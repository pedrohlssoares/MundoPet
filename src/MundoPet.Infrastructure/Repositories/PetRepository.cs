using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private const string Colunas =
        "idpet AS Id, nome AS Nome, especie AS Especie, raca AS Raca, idade AS Idade, clientepetfk AS ClienteId";

    private readonly DbConnectionFactory _db;

    public PetRepository(DbConnectionFactory db) => _db = db;

    public async Task<IEnumerable<Pet>> ObterTodosAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QueryAsync<Pet>("SELECT " + Colunas + " FROM pet ORDER BY nome");
    }

    public async Task<Pet?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Pet>("SELECT " + Colunas + " FROM pet WHERE idpet = @id", new { id });
    }

    public async Task<int> AdicionarAsync(Pet item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO pet (nome, especie, raca, idade, clientepetfk) " +
            "VALUES (@Nome, @Especie, @Raca, @Idade, @ClienteId); SELECT LAST_INSERT_ID();", item);
    }

    public async Task<bool> AtualizarAsync(Pet item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync(
            "UPDATE pet SET nome = @Nome, especie = @Especie, raca = @Raca, idade = @Idade, " +
            "clientepetfk = @ClienteId WHERE idpet = @Id", item) > 0;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteAsync("DELETE FROM pet WHERE idpet = @id", new { id }) > 0;
    }

    /// <summary>Número = busca por ID; texto = busca por parte do nome.</summary>
    public async Task<IEnumerable<Pet>> BuscarAsync(string termo)
    {
        using var con = _db.CriarConexao();

        if (int.TryParse(termo, out var id))
        {
            return await con.QueryAsync<Pet>("SELECT " + Colunas + " FROM pet WHERE idpet = @id", new { id });
        }

        return await con.QueryAsync<Pet>(
            "SELECT " + Colunas + " FROM pet WHERE nome LIKE @termo ORDER BY nome LIMIT 20",
            new { termo = "%" + termo + "%" });
    }
}
