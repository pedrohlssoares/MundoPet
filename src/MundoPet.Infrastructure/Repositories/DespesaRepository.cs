using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class DespesaRepository : IDespesaRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Despesa.
    private const string Colunas =
        "iddespesa AS Id, preco AS Preco, despesadata AS Data, descricao AS Descricao";

    private readonly DbConnectionFactory _db;

    public DespesaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Despesa>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Despesa>("SELECT " + Colunas + " FROM despesa ORDER BY despesadata DESC");
    }

    public async Task<Despesa?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Despesa>(
            "SELECT " + Colunas + " FROM despesa WHERE iddespesa = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Despesa item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO despesa (preco, despesadata, descricao) VALUES (@Preco, @Data, @Descricao); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Despesa item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE despesa SET preco = @Preco, despesadata = @Data, descricao = @Descricao WHERE iddespesa = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM despesa WHERE iddespesa = @id", new { id }) > 0;
    }
}
