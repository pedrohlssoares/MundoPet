using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class SalaRepository : ISalaRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Sala.
    private const string Colunas =
        "numero AS Numero, tipo AS Tipo, descricao AS Descricao";

    private readonly DbConnectionFactory _db;

    public SalaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Sala>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Sala>("SELECT " + Colunas + " FROM sala ORDER BY numero");
    }

    public async Task<Sala?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Sala>(
            "SELECT " + Colunas + " FROM sala WHERE numero = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Sala item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO sala (tipo, descricao) VALUES (@Tipo, @Descricao); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Sala item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE sala SET tipo = @Tipo, descricao = @Descricao WHERE numero = @Numero", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM sala WHERE numero = @id", new { id }) > 0;
    }
}
