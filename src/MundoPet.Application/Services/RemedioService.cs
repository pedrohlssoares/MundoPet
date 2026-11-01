using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IRemedioService
{
    Task<Result<IEnumerable<RemedioDto>>> ListarAsync();
    Task<Result<RemedioDto>> ObterAsync(int id);
    Task<Result<RemedioDto>> CriarAsync(RemedioDto dto);
    Task<Result<RemedioDto>> AtualizarAsync(int id, RemedioDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class RemedioService : IRemedioService
{
    private readonly IRemedioRepository _repo;

    public RemedioService(IRemedioRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<RemedioDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<RemedioDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<RemedioDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<RemedioDto>.NotFound(id);

        return Result<RemedioDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<RemedioDto>> CriarAsync(RemedioDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<RemedioDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<RemedioDto>> AtualizarAsync(int id, RemedioDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<RemedioDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<RemedioDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static RemedioDto ParaDto(Remedio e) => new() { Id = e.Id, ProdutoId = e.ProdutoId, Ativo = e.Ativo, Lote = e.Lote };

    private static Remedio ParaEntidade(RemedioDto d) => new() { Id = d.Id, ProdutoId = d.ProdutoId, Ativo = d.Ativo, Lote = d.Lote };
}
