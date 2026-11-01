using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IPrescricaoService
{
    Task<Result<IEnumerable<PrescricaoDto>>> ListarAsync();
    Task<Result<PrescricaoDto>> ObterAsync(int id);
    Task<Result<PrescricaoDto>> CriarAsync(PrescricaoDto dto);
    Task<Result<PrescricaoDto>> AtualizarAsync(int id, PrescricaoDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

/// <summary>RN002 – Imutabilidade do prontuário: prescrições não podem ser excluídas.</summary>
public class PrescricaoService : IPrescricaoService
{
    private readonly IPrescricaoRepository _repo;

    public PrescricaoService(IPrescricaoRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<PrescricaoDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<PrescricaoDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<PrescricaoDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<PrescricaoDto>.NotFound(id);

        return Result<PrescricaoDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<PrescricaoDto>> CriarAsync(PrescricaoDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<PrescricaoDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<PrescricaoDto>> AtualizarAsync(int id, PrescricaoDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<PrescricaoDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<PrescricaoDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public Task<Result<object?>> ExcluirAsync(int id)
    {
        var bloqueado = Result<object?>.Fail(405, "Operação não permitida.",
            "Não é permitida a exclusão de prescrições médicas por motivos de auditoria clínica.");
        return Task.FromResult(bloqueado);
    }

    private static PrescricaoDto ParaDto(Prescricao e) => new() { Id = e.Id, ConsultaId = e.ConsultaId, RemedioId = e.RemedioId, VacinaId = e.VacinaId, Dosagem = e.Dosagem };

    private static Prescricao ParaEntidade(PrescricaoDto d) => new() { Id = d.Id, ConsultaId = d.ConsultaId, RemedioId = d.RemedioId, VacinaId = d.VacinaId, Dosagem = d.Dosagem };
}
