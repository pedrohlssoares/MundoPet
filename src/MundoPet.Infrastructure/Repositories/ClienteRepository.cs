using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

/// <summary>Cliente + endereço: o endereço fica em outra tabela e é gravado junto (sem transação, só dois INSERTs simples).</summary>
public class ClienteRepository : IClienteRepository
{
    private const string SelectSql =
        "SELECT c.idcliente AS Id, c.nome AS Nome, c.cpf AS Cpf, c.email AS Email, c.celular AS Celular, " +
        "c.enderecoclientefk AS EnderecoId, " +
        "e.idendereco AS Id, e.rua AS Rua, e.cidade AS Cidade, e.bairro AS Bairro, e.numero AS Numero, " +
        "e.complemento AS Complemento " +
        "FROM cliente c INNER JOIN endereco e ON e.idendereco = c.enderecoclientefk";

    private const string InsertSql =
        "INSERT INTO cliente (nome, cpf, email, celular, enderecoclientefk) " +
        "VALUES (@Nome, @Cpf, @Email, @Celular, @EnderecoId)";

    private const string UpdateSql =
        "UPDATE cliente SET nome = @Nome, cpf = @Cpf, email = @Email, celular = @Celular, " +
        "enderecoclientefk = @EnderecoId WHERE idcliente = @Id";

    private readonly DbConnectionFactory _db;

    public ClienteRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Cliente, Endereco, Cliente>(
            SelectSql + " ORDER BY c.nome",
            (c, endereco) => { c.Endereco = endereco; return c; },
            splitOn: "Id");
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        var lista = await con.QueryAsync<Cliente, Endereco, Cliente>(
            SelectSql + " WHERE c.idcliente = @id",
            (c, endereco) => { c.Endereco = endereco; return c; },
            new { id },
            splitOn: "Id");
        return lista.FirstOrDefault();
    }

    public async Task<int> AdicionarAsync(Cliente item)
    {
        using var con = _db.CriarConexao();

        // 1º grava o endereço e pega o ID gerado; 2º grava o registro apontando para esse endereço.
        item.EnderecoId = await con.ExecuteScalarAsync<int>(
            EnderecoSql.Inserir + "; SELECT LAST_INSERT_ID();", item.Endereco);

        return await con.ExecuteScalarAsync<int>(InsertSql + "; SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Cliente item)
    {
        using var con = _db.CriarConexao();

        var enderecoId = await con.QuerySingleOrDefaultAsync<int?>(
            "SELECT enderecoclientefk FROM cliente WHERE idcliente = @Id", new { item.Id });
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
            "SELECT enderecoclientefk FROM cliente WHERE idcliente = @id", new { id });
        if (enderecoId is null) return false;

        // Se houver registros ligados (FK), o banco recusa e a API responde 409.
        await con.ExecuteAsync("DELETE FROM cliente WHERE idcliente = @id", new { id });
        await con.ExecuteAsync(EnderecoSql.ApagarSeOrfao, new { enderecoId });
        return true;
    }
}
