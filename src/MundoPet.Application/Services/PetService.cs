using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IPetService
{
    Task<Result<IEnumerable<PetDto>>> ListarAsync();
    Task<Result<PetDto>> ObterAsync(int id);
    Task<Result<PetDto>> CriarAsync(PetDto dto);
    Task<Result<PetDto>> AtualizarAsync(int id, PetDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
    Task<Result<IEnumerable<PetDto>>> BuscarAsync(string? termo);
    Task<Result<HistoricoPetDto>> HistoricoAsync(int id);
}

public class PetService : IPetService
{
    private readonly IPetRepository _repo;
    private readonly IHistoricoPetRepository _historico;

    public PetService(IPetRepository repo, IHistoricoPetRepository historico)
    {
        _repo = repo;
        _historico = historico;
    }

    public async Task<Result<IEnumerable<PetDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<PetDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<PetDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<PetDto>.NotFound(id);

        return Result<PetDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<PetDto>> CriarAsync(PetDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<PetDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<PetDto>> AtualizarAsync(int id, PetDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<PetDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<PetDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    /// <summary>Busca por ID (se o termo for número) ou por parte do nome.</summary>
    public async Task<Result<IEnumerable<PetDto>>> BuscarAsync(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return Result<IEnumerable<PetDto>>.Fail(400, "Termo de busca obrigatório.",
                "Informe o nome ou o ID do pet.");
        }

        var pets = (await _repo.BuscarAsync(termo.Trim())).Select(ParaDto).ToList();
        return Result<IEnumerable<PetDto>>.Ok(pets, $"{pets.Count} pet(s) encontrado(s).");
    }

    /// <summary>Painel do pet: tutor, consultas, vacinas, vendas e total gasto.</summary>
    public async Task<Result<HistoricoPetDto>> HistoricoAsync(int id)
    {
        var historico = await _historico.ObterAsync(id);
        if (historico is null) return Result<HistoricoPetDto>.NotFound(id);

        return Result<HistoricoPetDto>.Ok(historico, "Histórico do pet carregado com sucesso.");
    }

    private static PetDto ParaDto(Pet e) => new()
    {
        Id = e.Id, Nome = e.Nome, Especie = e.Especie, Raca = e.Raca, Idade = e.Idade, ClienteId = e.ClienteId
    };

    private static Pet ParaEntidade(PetDto d) => new()
    {
        Id = d.Id, Nome = d.Nome, Especie = d.Especie, Raca = d.Raca, Idade = d.Idade, ClienteId = d.ClienteId
    };
}
