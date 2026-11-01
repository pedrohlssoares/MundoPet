using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class VendaRepository : IVendaRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Venda.
    private const string Colunas =
        "idvenda AS Id, produtovendafk AS ProdutoId, quantidade AS Quantidade, valor_unitario AS ValorUnitario";

    private readonly DbConnectionFactory _db;

    public VendaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Venda>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Venda>("SELECT " + Colunas + " FROM venda ORDER BY idvenda DESC");
    }

    public async Task<Venda?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Venda>(
            "SELECT " + Colunas + " FROM venda WHERE idvenda = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Venda item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO venda (produtovendafk, quantidade, valor_unitario) VALUES (@ProdutoId, @Quantidade, @ValorUnitario); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Venda item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE venda SET produtovendafk = @ProdutoId, quantidade = @Quantidade, valor_unitario = @ValorUnitario WHERE idvenda = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM venda WHERE idvenda = @id", new { id }) > 0;
    }
}
