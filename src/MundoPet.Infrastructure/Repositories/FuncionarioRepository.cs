using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

/// <summary>Funcionario + endereço: o endereço fica em outra tabela e é gravado junto (sem transação, só dois INSERTs simples).</summary>
public class FuncionarioRepository : IFuncionarioRepository
{
    private const string SelectSql =
        "SELECT f.idfuncionario AS Id, f.nome AS Nome, f.celular AS Celular, f.cpf AS Cpf, f.salario AS Salario, " +
        "f.enderecofuncionariofk AS EnderecoId, " +
        "e.idendereco AS Id, e.rua AS Rua, e.cidade AS Cidade, e.bairro AS Bairro, e.numero AS Numero, " +
        "e.complemento AS Complemento " +
        "FROM funcionario f INNER JOIN endereco e ON e.idendereco = f.enderecofuncionariofk";

    private const string InsertSql =
        "INSERT INTO funcionario (nome, celular, cpf, salario, enderecofuncionariofk) " +
        "VALUES (@Nome, @Celular, @Cpf, @Salario, @EnderecoId)";

    private const string UpdateSql =
        "UPDATE funcionario SET nome = @Nome, celular = @Celular, cpf = @Cpf, salario = @Salario, " +
        "enderecofuncionariofk = @EnderecoId WHERE idfuncionario = @Id";

    private readonly DbConnectionFactory _db;

    public FuncionarioRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Funcionario>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Funcionario, Endereco, Funcionario>(
            SelectSql + " ORDER BY f.nome",
            (f, endereco) => { f.Endereco = endereco; return f; },
            splitOn: "Id");
    }

    public async Task<Funcionario?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        var lista = await con.QueryAsync<Funcionario, Endereco, Funcionario>(
            SelectSql + " WHERE f.idfuncionario = @id",
            (f, endereco) => { f.Endereco = endereco; return f; },
            new { id },
            splitOn: "Id");
        return lista.FirstOrDefault();
    }

    public async Task<int> AdicionarAsync(Funcionario item)
    {
        using var con = _db.CriarConexao();

        // 1º grava o endereço e pega o ID gerado; 2º grava o registro apontando para esse endereço.
        item.EnderecoId = await con.ExecuteScalarAsync<int>(
            EnderecoSql.Inserir + "; SELECT LAST_INSERT_ID();", item.Endereco);

        return await con.ExecuteScalarAsync<int>(InsertSql + "; SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Funcionario item)
    {
        using var con = _db.CriarConexao();

        var enderecoId = await con.QuerySingleOrDefaultAsync<int?>(
            "SELECT enderecofuncionariofk FROM funcionario WHERE idfuncionario = @Id", new { item.Id });
        if (enderecoId is null) return false;

        item.EnderecoId = enderecoId.Value;
        item.Endereco.Id = enderecoId.Value;

        await con.ExecuteAsync(EnderecoSql.Atualizar, item.Endereco);
        await con.ExecuteAsync(UpdateSql, item);
        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();

        var enderecoId = await con.QuerySingleOrDefaultAsync<int?>(
            "SELECT enderecofuncionariofk FROM funcionario WHERE idfuncionario = @id", new { id });
        if (enderecoId is null) return false;

        // Se houver registros ligados (FK), o banco recusa e a API responde 409.
        await con.ExecuteAsync("DELETE FROM funcionario WHERE idfuncionario = @id", new { id });
        await con.ExecuteAsync(EnderecoSql.ApagarSeOrfao, new { enderecoId });
        return true;
    }
}
