using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class HistoricoVacinacaoRepository : IHistoricoVacinacaoRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe HistoricoVacinacao.
    private const string Colunas =
        "idhistorico AS Id, pethistorico_vacinacaofk AS PetId, vacinahistorico_vacinacaofk AS VacinaId, data_aplicacao AS DataAplicacao, dosagem AS Dosagem";

    private readonly DbConnectionFactory _db;

    public HistoricoVacinacaoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<HistoricoVacinacao>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<HistoricoVacinacao>("SELECT " + Colunas + " FROM historico_vacinacao ORDER BY data_aplicacao DESC");
    }

    public async Task<HistoricoVacinacao?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<HistoricoVacinacao>(
            "SELECT " + Colunas + " FROM historico_vacinacao WHERE idhistorico = @id", new { id });
    }

    public async Task<int> AdicionarAsync(HistoricoVacinacao item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO historico_vacinacao (pethistorico_vacinacaofk, vacinahistorico_vacinacaofk, data_aplicacao, dosagem) VALUES (@PetId, @VacinaId, @DataAplicacao, @Dosagem); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(HistoricoVacinacao item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE historico_vacinacao SET pethistorico_vacinacaofk = @PetId, vacinahistorico_vacinacaofk = @VacinaId, data_aplicacao = @DataAplicacao, dosagem = @Dosagem WHERE idhistorico = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM historico_vacinacao WHERE idhistorico = @id", new { id }) > 0;
    }
}
