using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IVeterinarioService
{
    Task<Result<IEnumerable<VeterinarioDto>>> ListarAsync();
    Task<Result<VeterinarioDto>> ObterAsync(int id);
    Task<Result<VeterinarioDto>> CriarAsync(VeterinarioDto dto);
    Task<Result<VeterinarioDto>> AtualizarAsync(int id, VeterinarioDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

/// <summary>Veterinário é um funcionário já cadastrado. O DTO devolve também o Nome (vem do JOIN no repositório).</summary>
public class VeterinarioService : IVeterinarioService
{
    private readonly IVeterinarioRepository _repo;

    public VeterinarioService(IVeterinarioRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<VeterinarioDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<VeterinarioDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<VeterinarioDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<VeterinarioDto>.NotFound(id);

        return Result<VeterinarioDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<VeterinarioDto>> CriarAsync(VeterinarioDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<VeterinarioDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<VeterinarioDto>> AtualizarAsync(int id, VeterinarioDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<VeterinarioDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<VeterinarioDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static VeterinarioDto ParaDto(Veterinario e) => new() { Id = e.Id, Crmv = e.Crmv, FuncionarioId = e.FuncionarioId, Descricao = e.Descricao, Nome = e.Nome };

    private static Veterinario ParaEntidade(VeterinarioDto d) => new() { Id = d.Id, Crmv = d.Crmv, FuncionarioId = d.FuncionarioId, Descricao = d.Descricao };
}
