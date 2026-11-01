using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Domain.Common;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IConsultaService
{
    Task<Result<IEnumerable<ConsultaDto>>> ListarAsync();
    Task<Result<ConsultaDto>> ObterAsync(int id);
    Task<Result<ConsultaDto>> CriarAsync(ConsultaDto dto);
    Task<Result<ConsultaDto>> AtualizarAsync(int id, ConsultaDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
    Task<Result<IEnumerable<ConsultaDto>>> ListarPorDataAsync(DateTime? data);
    Task<Result<ConsultaDto>> AlterarStatusAsync(int id, string status);
}

public class ConsultaService : IConsultaService
{
    private readonly IConsultaRepository _repo;

    public ConsultaService(IConsultaRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<ConsultaDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<ConsultaDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<ConsultaDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<ConsultaDto>.NotFound(id);

        return Result<ConsultaDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<ConsultaDto>> CriarAsync(ConsultaDto dto)
    {
        var item = ParaEntidade(dto);

        var conflito = await VerificarConflitoAsync(item, 0);
        if (conflito is not null) return conflito;

        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<ConsultaDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<ConsultaDto>> AtualizarAsync(int id, ConsultaDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<ConsultaDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;

        var conflito = await VerificarConflitoAsync(item, id);
        if (conflito is not null) return conflito;

        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<ConsultaDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    // RN002 – Imutabilidade do prontuário: consultas não podem ser excluídas.
    public Task<Result<object?>> ExcluirAsync(int id)
    {
        var bloqueado = Result<object?>.Fail(405, "Operação não permitida.",
            "Não é permitida a exclusão de prontuários e consultas por motivos de auditoria clínica. Altere o status para Cancelada.");
        return Task.FromResult(bloqueado);
    }

    public async Task<Result<IEnumerable<ConsultaDto>>> ListarPorDataAsync(DateTime? data)
    {
        if (data is null)
        {
            return Result<IEnumerable<ConsultaDto>>.Fail(400, "Data obrigatória.",
                "Informe a data no formato aaaa-mm-dd (ex.: ?data=2026-06-04).");
        }

        var lista = (await _repo.ObterPorDataAsync(data.Value.Date)).Select(ParaDto).ToList();
        return Result<IEnumerable<ConsultaDto>>.Ok(lista, $"{lista.Count} consulta(s) na data informada.");
    }

    /// <summary>"Status rápido" da agenda: Agendada, Realizada ou Cancelada.</summary>
    public async Task<Result<ConsultaDto>> AlterarStatusAsync(int id, string status)
    {
        var consulta = await _repo.ObterPorIdAsync(id);
        if (consulta is null) return Result<ConsultaDto>.NotFound(id);

        consulta.Status = status;

        // Reabrir uma consulta cancelada pode gerar conflito de agenda.
        var conflito = await VerificarConflitoAsync(consulta, id);
        if (conflito is not null) return conflito;

        await _repo.AtualizarAsync(consulta);
        return Result<ConsultaDto>.Ok(ParaDto(consulta), $"Status alterado para {status}.");
    }

    // RN004 – Exclusividade de agenda: o mesmo veterinário ou a mesma sala não podem ter
    // dois atendimentos no mesmo dia e horário (consultas canceladas não contam).
    private async Task<Result<ConsultaDto>?> VerificarConflitoAsync(Consulta c, int ignorarId)
    {
        if (c.Status == StatusConsulta.Cancelada) return null;

        if (await _repo.VeterinarioOcupadoAsync(c.VeterinarioId, c.DataConsulta, c.Horario, ignorarId))
        {
            return Result<ConsultaDto>.Fail(409, "Conflito de agenda.",
                "O veterinário já possui uma consulta nesse dia e horário.");
        }

        if (await _repo.SalaOcupadaAsync(c.SalaId, c.DataConsulta, c.Horario, ignorarId))
        {
            return Result<ConsultaDto>.Fail(409, "Conflito de agenda.",
                "A sala já está reservada nesse dia e horário.");
        }

        return null;
    }

    private static ConsultaDto ParaDto(Consulta e) => new()
    {
        Id = e.Id, PetId = e.PetId, VeterinarioId = e.VeterinarioId, SalaId = e.SalaId,
        DataConsulta = e.DataConsulta, Horario = e.Horario, Status = e.Status, ProcessosFeitos = e.ProcessosFeitos
    };

    private static Consulta ParaEntidade(ConsultaDto d) => new()
    {
        Id = d.Id, PetId = d.PetId, VeterinarioId = d.VeterinarioId, SalaId = d.SalaId,
        DataConsulta = d.DataConsulta!.Value.Date, Horario = d.Horario!.Value,
        Status = d.Status, ProcessosFeitos = d.ProcessosFeitos ?? string.Empty
    };
}
