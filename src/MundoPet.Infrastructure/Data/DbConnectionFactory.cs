using MySqlConnector;

namespace MundoPet.Infrastructure.Data;

/// <summary>
/// Cria conexões com o MySQL. Cada repositório faz:  using var con = _db.CriarConexao();
/// O Dapper abre a conexão sozinho na primeira consulta e o "using" a fecha no final.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString) => _connectionString = connectionString;

    public MySqlConnection CriarConexao() => new(_connectionString);
}
