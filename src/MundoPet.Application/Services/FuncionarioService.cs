using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IFuncionarioService
{
    Task<Result<IEnumerable<FuncionarioDto>>> ListarAsync();
    Task<Result<FuncionarioDto>> ObterAsync(int id);
    Task<Result<FuncionarioDto>> CriarAsync(FuncionarioDto dto);
    Task<Result<FuncionarioDto>> AtualizarAsync(int id, FuncionarioDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class FuncionarioService : IFuncionarioService
{
    private readonly IFuncionarioRepository _repo;

    public FuncionarioService(IFuncionarioRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<FuncionarioDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<FuncionarioDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<FuncionarioDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<FuncionarioDto>.NotFound(id);

        return Result<FuncionarioDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<FuncionarioDto>> CriarAsync(FuncionarioDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<FuncionarioDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<FuncionarioDto>> AtualizarAsync(int id, FuncionarioDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<FuncionarioDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<FuncionarioDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static FuncionarioDto ParaDto(Funcionario e) => new() { Id = e.Id, Nome = e.Nome, Celular = e.Celular, Cpf = e.Cpf, Salario = e.Salario, EnderecoId = e.EnderecoId, Endereco = EnderecoMapper.ParaDto(e.Endereco) };

    private static Funcionario ParaEntidade(FuncionarioDto d) => new() { Id = d.Id, Nome = d.Nome, Celular = d.Celular, Cpf = d.Cpf, Salario = d.Salario, EnderecoId = d.EnderecoId, Endereco = EnderecoMapper.ParaEntidade(d.Endereco) };
}
