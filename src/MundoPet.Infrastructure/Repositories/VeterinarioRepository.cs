using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class VeterinarioRepository : IVeterinarioRepository
{
    // JOIN com funcionario para trazer o nome do veterinário junto.
    private const string SelectSql =
        "SELECT v.idveterinario AS Id, v.crmv AS Crmv, v.funcionarioveterinariofk AS FuncionarioId, " +
        "v.descricao AS Descricao, f.nome AS Nome " +
        "FROM veterinario v INNER JOIN funcionario f ON f.idfuncionario = v.funcionarioveterinariofk";

    private readonly DbConnectionFactory _db;

    public VeterinarioRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Veterinario>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Veterinario>(SelectSql + " ORDER BY f.nome");
    }

    public async Task<Veterinario?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Veterinario>(SelectSql + " WHERE v.idveterinario = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Veterinario item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO veterinario (crmv, funcionarioveterinariofk, descricao) " +
            "VALUES (@Crmv, @FuncionarioId, @Descricao); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Veterinario item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE veterinario SET crmv = @Crmv, funcionarioveterinariofk = @FuncionarioId, " +
            "descricao = @Descricao WHERE idveterinario = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM veterinario WHERE idveterinario = @id", new { id }) > 0;
    }
}
