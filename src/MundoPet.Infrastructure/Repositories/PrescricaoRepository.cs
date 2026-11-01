using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class PrescricaoRepository : IPrescricaoRepository
{
    // As colunas do banco recebem "apelidos" (AS) iguais às propriedades da classe Prescricao.
    private const string Colunas =
        "idprescricao AS Id, consultaprescricaofk AS ConsultaId, remedioprescricaofk AS RemedioId, vacinaprescricaofk AS VacinaId, dosagem AS Dosagem";

    private readonly DbConnectionFactory _db;

    public PrescricaoRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Prescricao>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Prescricao>("SELECT " + Colunas + " FROM prescricao ORDER BY idprescricao DESC");
    }

    public async Task<Prescricao?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Prescricao>(
            "SELECT " + Colunas + " FROM prescricao WHERE idprescricao = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Prescricao item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO prescricao (consultaprescricaofk, remedioprescricaofk, vacinaprescricaofk, dosagem) VALUES (@ConsultaId, @RemedioId, @VacinaId, @Dosagem); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Prescricao item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE prescricao SET consultaprescricaofk = @ConsultaId, remedioprescricaofk = @RemedioId, vacinaprescricaofk = @VacinaId, dosagem = @Dosagem WHERE idprescricao = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM prescricao WHERE idprescricao = @id", new { id }) > 0;
    }
}
