using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IFormaPagamentoService
{
    Task<Result<IEnumerable<FormaPagamentoDto>>> ListarAsync();
    Task<Result<FormaPagamentoDto>> ObterAsync(int id);
    Task<Result<FormaPagamentoDto>> CriarAsync(FormaPagamentoDto dto);
    Task<Result<FormaPagamentoDto>> AtualizarAsync(int id, FormaPagamentoDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class FormaPagamentoService : IFormaPagamentoService
{
    private readonly IFormaPagamentoRepository _repo;

    public FormaPagamentoService(IFormaPagamentoRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<FormaPagamentoDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<FormaPagamentoDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<FormaPagamentoDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<FormaPagamentoDto>.NotFound(id);

        return Result<FormaPagamentoDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<FormaPagamentoDto>> CriarAsync(FormaPagamentoDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<FormaPagamentoDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<FormaPagamentoDto>> AtualizarAsync(int id, FormaPagamentoDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<FormaPagamentoDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<FormaPagamentoDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static FormaPagamentoDto ParaDto(FormaPagamento e) => new() { Id = e.Id, Tipo = e.Tipo, Descricao = e.Descricao };

    private static FormaPagamento ParaEntidade(FormaPagamentoDto d) => new() { Id = d.Id, Tipo = d.Tipo, Descricao = d.Descricao };
}
