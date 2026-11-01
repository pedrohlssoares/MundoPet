using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class VacinaRepository : IVacinaRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Vacina.
    private const string Colunas =
        "idvacina AS Id, produtovacinafk AS ProdutoId, ativo AS Ativo, lote AS Lote";

    private readonly DbConnectionFactory _db;

    public VacinaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Vacina>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Vacina>("SELECT " + Colunas + " FROM vacina ORDER BY ativo");
    }

    public async Task<Vacina?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Vacina>(
            "SELECT " + Colunas + " FROM vacina WHERE idvacina = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Vacina item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO vacina (produtovacinafk, ativo, lote) VALUES (@ProdutoId, @Ativo, @Lote); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Vacina item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE vacina SET produtovacinafk = @ProdutoId, ativo = @Ativo, lote = @Lote WHERE idvacina = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM vacina WHERE idvacina = @id", new { id }) > 0;
    }
}
