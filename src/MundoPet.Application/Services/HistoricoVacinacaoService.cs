using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IHistoricoVacinacaoService
{
    Task<Result<IEnumerable<HistoricoVacinacaoDto>>> ListarAsync();
    Task<Result<HistoricoVacinacaoDto>> ObterAsync(int id);
    Task<Result<HistoricoVacinacaoDto>> CriarAsync(HistoricoVacinacaoDto dto);
    Task<Result<HistoricoVacinacaoDto>> AtualizarAsync(int id, HistoricoVacinacaoDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class HistoricoVacinacaoService : IHistoricoVacinacaoService
{
    private readonly IHistoricoVacinacaoRepository _repo;

    public HistoricoVacinacaoService(IHistoricoVacinacaoRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<HistoricoVacinacaoDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<HistoricoVacinacaoDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<HistoricoVacinacaoDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<HistoricoVacinacaoDto>.NotFound(id);

        return Result<HistoricoVacinacaoDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<HistoricoVacinacaoDto>> CriarAsync(HistoricoVacinacaoDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<HistoricoVacinacaoDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<HistoricoVacinacaoDto>> AtualizarAsync(int id, HistoricoVacinacaoDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<HistoricoVacinacaoDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<HistoricoVacinacaoDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static HistoricoVacinacaoDto ParaDto(HistoricoVacinacao e) => new() { Id = e.Id, PetId = e.PetId, VacinaId = e.VacinaId, DataAplicacao = e.DataAplicacao, Dosagem = e.Dosagem };

    private static HistoricoVacinacao ParaEntidade(HistoricoVacinacaoDto d) => new() { Id = d.Id, PetId = d.PetId, VacinaId = d.VacinaId, DataAplicacao = d.DataAplicacao!.Value, Dosagem = d.Dosagem };
}
