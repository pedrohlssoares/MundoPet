using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Produto.
    private const string Colunas =
        "idproduto AS Id, nome AS Nome, quantidade AS Quantidade, descricao AS Descricao, preco AS Preco";

    private readonly DbConnectionFactory _db;

    public ProdutoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Produto>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Produto>("SELECT " + Colunas + " FROM produto ORDER BY nome");
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Produto>(
            "SELECT " + Colunas + " FROM produto WHERE idproduto = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Produto item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO produto (nome, quantidade, descricao, preco) VALUES (@Nome, @Quantidade, @Descricao, @Preco); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Produto item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE produto SET nome = @Nome, quantidade = @Quantidade, descricao = @Descricao, preco = @Preco WHERE idproduto = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM produto WHERE idproduto = @id", new { id }) > 0;
    }
}
