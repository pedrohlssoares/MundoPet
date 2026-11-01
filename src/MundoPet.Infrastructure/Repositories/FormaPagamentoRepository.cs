using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class FormaPagamentoRepository : IFormaPagamentoRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe FormaPagamento.
    private const string Colunas =
        "idforma_pagamento AS Id, tipo AS Tipo, descricao AS Descricao";

    private readonly DbConnectionFactory _db;

    public FormaPagamentoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<FormaPagamento>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<FormaPagamento>("SELECT " + Colunas + " FROM forma_pagamento ORDER BY tipo");
    }

    public async Task<FormaPagamento?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<FormaPagamento>(
            "SELECT " + Colunas + " FROM forma_pagamento WHERE idforma_pagamento = @id", new { id });
    }

    public async Task<int> AdicionarAsync(FormaPagamento item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO forma_pagamento (tipo, descricao) VALUES (@Tipo, @Descricao); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(FormaPagamento item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE forma_pagamento SET tipo = @Tipo, descricao = @Descricao WHERE idforma_pagamento = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM forma_pagamento WHERE idforma_pagamento = @id", new { id }) > 0;
    }
}
