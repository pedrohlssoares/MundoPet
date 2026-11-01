using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class PagamentoRepository : IPagamentoRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Pagamento.
    private const string Colunas =
        "idpagamento AS Id, prestacoes AS Prestacoes, valor AS Valor, data_pagamento AS DataPagamento, formapagamentofk AS FormaPagamentoId, clientepagamentofk AS ClienteId, consultapagamentofk AS ConsultaId, prescricaopagamentofk AS PrescricaoId, vendapagamentofk AS VendaId";

    private readonly DbConnectionFactory _db;

    public PagamentoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Pagamento>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Pagamento>("SELECT " + Colunas + " FROM pagamento ORDER BY data_pagamento DESC");
    }

    public async Task<Pagamento?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Pagamento>(
            "SELECT " + Colunas + " FROM pagamento WHERE idpagamento = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Pagamento item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO pagamento (prestacoes, valor, data_pagamento, formapagamentofk, clientepagamentofk, consultapagamentofk, prescricaopagamentofk, vendapagamentofk) VALUES (@Prestacoes, @Valor, COALESCE(@DataPagamento, NOW()), @FormaPagamentoId, @ClienteId, @ConsultaId, @PrescricaoId, @VendaId); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Pagamento item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE pagamento SET prestacoes = @Prestacoes, valor = @Valor, data_pagamento = COALESCE(@DataPagamento, data_pagamento), formapagamentofk = @FormaPagamentoId, clientepagamentofk = @ClienteId, consultapagamentofk = @ConsultaId, prescricaopagamentofk = @PrescricaoId, vendapagamentofk = @VendaId WHERE idpagamento = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM pagamento WHERE idpagamento = @id", new { id }) > 0;
    }
}
