using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IVacinaService
{
    Task<Result<IEnumerable<VacinaDto>>> ListarAsync();
    Task<Result<VacinaDto>> ObterAsync(int id);
    Task<Result<VacinaDto>> CriarAsync(VacinaDto dto);
    Task<Result<VacinaDto>> AtualizarAsync(int id, VacinaDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class VacinaService : IVacinaService
{
    private readonly IVacinaRepository _repo;

    public VacinaService(IVacinaRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<VacinaDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<VacinaDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<VacinaDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<VacinaDto>.NotFound(id);

        return Result<VacinaDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<VacinaDto>> CriarAsync(VacinaDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<VacinaDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<VacinaDto>> AtualizarAsync(int id, VacinaDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<VacinaDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<VacinaDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static VacinaDto ParaDto(Vacina e) => new() { Id = e.Id, ProdutoId = e.ProdutoId, Ativo = e.Ativo, Lote = e.Lote };

    private static Vacina ParaEntidade(VacinaDto d) => new() { Id = d.Id, ProdutoId = d.ProdutoId, Ativo = d.Ativo, Lote = d.Lote };
}
