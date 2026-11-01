using Dapper;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private const string Colunas =
        "idusuario AS Id, nome AS Nome, email AS Email, senha_hash AS SenhaHash, criado_em AS CriadoEm";

    private readonly DbConnectionFactory _db;

    public UsuarioRepository(DbConnectionFactory db) => _db = db;

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT " + Colunas + " FROM usuario WHERE idusuario = @id", new { id });
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT " + Colunas + " FROM usuario WHERE email = @email", new { email });
    }

    public async Task<int> AdicionarAsync(Usuario item)
    {
        using var con = _db.CriarConexao();
        return await con.ExecuteScalarAsync<int>(
            "INSERT INTO usuario (nome, email, senha_hash) VALUES (@Nome, @Email, @SenhaHash); " +
            "SELECT LAST_INSERT_ID();", item);
    }
}
