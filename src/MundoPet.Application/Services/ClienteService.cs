using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IClienteService
{
    Task<Result<IEnumerable<ClienteDto>>> ListarAsync();
    Task<Result<ClienteDto>> ObterAsync(int id);
    Task<Result<ClienteDto>> CriarAsync(ClienteDto dto);
    Task<Result<ClienteDto>> AtualizarAsync(int id, ClienteDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<ClienteDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<ClienteDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<ClienteDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<ClienteDto>.NotFound(id);

        return Result<ClienteDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<ClienteDto>> CriarAsync(ClienteDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<ClienteDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<ClienteDto>> AtualizarAsync(int id, ClienteDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<ClienteDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<ClienteDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static ClienteDto ParaDto(Cliente e) => new() { Id = e.Id, Nome = e.Nome, Cpf = e.Cpf, Email = e.Email, Celular = e.Celular, EnderecoId = e.EnderecoId, Endereco = EnderecoMapper.ParaDto(e.Endereco) };

    private static Cliente ParaEntidade(ClienteDto d) => new() { Id = d.Id, Nome = d.Nome, Cpf = d.Cpf, Email = d.Email, Celular = d.Celular, EnderecoId = d.EnderecoId, Endereco = EnderecoMapper.ParaEntidade(d.Endereco) };
}
