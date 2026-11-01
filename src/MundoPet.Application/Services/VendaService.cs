using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IVendaService
{
    Task<Result<IEnumerable<VendaDto>>> ListarAsync();
    Task<Result<VendaDto>> ObterAsync(int id);
    Task<Result<VendaDto>> CriarAsync(VendaDto dto);
    Task<Result<VendaDto>> AtualizarAsync(int id, VendaDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class VendaService : IVendaService
{
    private readonly IVendaRepository _repo;

    public VendaService(IVendaRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<VendaDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<VendaDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<VendaDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<VendaDto>.NotFound(id);

        return Result<VendaDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<VendaDto>> CriarAsync(VendaDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<VendaDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<VendaDto>> AtualizarAsync(int id, VendaDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<VendaDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<VendaDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static VendaDto ParaDto(Venda e) => new() { Id = e.Id, ProdutoId = e.ProdutoId, Quantidade = e.Quantidade, ValorUnitario = e.ValorUnitario };

    private static Venda ParaEntidade(VendaDto d) => new() { Id = d.Id, ProdutoId = d.ProdutoId, Quantidade = d.Quantidade, ValorUnitario = d.ValorUnitario };
}
