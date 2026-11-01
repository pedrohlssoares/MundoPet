using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private const string Colunas =
        "idconsulta AS Id, petconsultafk AS PetId, veterinarioconsultafk AS VeterinarioId, " +
        "salaconsultafk AS SalaId, data_consulta AS DataConsulta, horario AS Horario, " +
        "status AS Status, processos_feitos AS ProcessosFeitos";

    private readonly DbConnectionFactory _db;

    public ConsultaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Consulta>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Consulta>(
            "SELECT " + Colunas + " FROM consulta ORDER BY data_consulta DESC, horario DESC");
    }

    public async Task<Consulta?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Consulta>(
            "SELECT " + Colunas + " FROM consulta WHERE idconsulta = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Consulta item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO consulta (petconsultafk, veterinarioconsultafk, salaconsultafk, data_consulta, horario, status, processos_feitos) " +
            "VALUES (@PetId, @VeterinarioId, @SalaId, @DataConsulta, @Horario, @Status, @ProcessosFeitos); " +
            "SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Consulta item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE consulta SET petconsultafk = @PetId, veterinarioconsultafk = @VeterinarioId, salaconsultafk = @SalaId, " +
            "data_consulta = @DataConsulta, horario = @Horario, status = @Status, processos_feitos = @ProcessosFeitos " +
            "WHERE idconsulta = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM consulta WHERE idconsulta = @id", new { id }) > 0;
    }

    public async Task<IEnumerable<Consulta>> ObterPorDataAsync(DateTime data)
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Consulta>(
            "SELECT " + Colunas + " FROM consulta WHERE data_consulta = @data ORDER BY horario", new { data });
    }

    // Há conflito se já existir outra consulta (não cancelada) do mesmo veterinário no mesmo dia e horário.
    public async Task<bool> VeterinarioOcupadoAsync(int veterinarioId, DateTime data, TimeSpan horario, int ignorarConsultaId)
    {
        using var con = _db.CriarConexao();
        var total = await con.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM consulta WHERE veterinarioconsultafk = @veterinarioId AND data_consulta = @data " +
            "AND horario = @horario AND status <> 'Cancelada' AND idconsulta <> @ignorarConsultaId",
            new { veterinarioId, data, horario, ignorarConsultaId });
        return total > 0;
    }

    // Idem para a sala.
    public async Task<bool> SalaOcupadaAsync(int salaId, DateTime data, TimeSpan horario, int ignorarConsultaId)
    {
        using var con = _db.CriarConexao();
        var total = await con.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM consulta WHERE salaconsultafk = @salaId AND data_consulta = @data " +
            "AND horario = @horario AND status <> 'Cancelada' AND idconsulta <> @ignorarConsultaId",
            new { salaId, data, horario, ignorarConsultaId });
        return total > 0;
    }
}
