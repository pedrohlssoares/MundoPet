using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IDespesaService
{
    Task<Result<IEnumerable<DespesaDto>>> ListarAsync();
    Task<Result<DespesaDto>> ObterAsync(int id);
    Task<Result<DespesaDto>> CriarAsync(DespesaDto dto);
    Task<Result<DespesaDto>> AtualizarAsync(int id, DespesaDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

/// <summary>Regra do sistema: despesa lançada não pode ser alterada.</summary>
public class DespesaService : IDespesaService
{
    private readonly IDespesaRepository _repo;

    public DespesaService(IDespesaRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<DespesaDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<DespesaDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<DespesaDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<DespesaDto>.NotFound(id);

        return Result<DespesaDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<DespesaDto>> CriarAsync(DespesaDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<DespesaDto>.Created(ParaDto(criado!));
    }

    public Task<Result<DespesaDto>> AtualizarAsync(int id, DespesaDto dto)
    {
        var bloqueado = Result<DespesaDto>.Fail(405, "Operação não permitida.",
            "Uma despesa lançada não pode ser alterada. Exclua e lance novamente, se necessário.");
        return Task.FromResult(bloqueado);
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static DespesaDto ParaDto(Despesa e) => new() { Id = e.Id, Preco = e.Preco, Data = e.Data, Descricao = e.Descricao };

    private static Despesa ParaEntidade(DespesaDto d) => new() { Id = d.Id, Preco = d.Preco, Data = d.Data!.Value, Descricao = d.Descricao };
}
