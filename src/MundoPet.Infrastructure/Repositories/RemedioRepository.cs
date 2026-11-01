using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class RemedioRepository : IRemedioRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Remedio.
    private const string Colunas =
        "idremedio AS Id, produtoremediofk AS ProdutoId, ativo AS Ativo, lote AS Lote";

    private readonly DbConnectionFactory _db;

    public RemedioRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Remedio>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Remedio>("SELECT " + Colunas + " FROM remedio ORDER BY ativo");
    }

    public async Task<Remedio?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Remedio>(
            "SELECT " + Colunas + " FROM remedio WHERE idremedio = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Remedio item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO remedio (produtoremediofk, ativo, lote) VALUES (@ProdutoId, @Ativo, @Lote); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Remedio item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE remedio SET produtoremediofk = @ProdutoId, ativo = @Ativo, lote = @Lote WHERE idremedio = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM remedio WHERE idremedio = @id", new { id }) > 0;
    }
}
