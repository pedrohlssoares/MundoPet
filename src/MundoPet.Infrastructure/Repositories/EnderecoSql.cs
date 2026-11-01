namespace MundoPet.Infrastructure.Repositories;

/// <summary>SQL da tabela endereco, reaproveitado por Cliente e Funcionário (que gravam o endereço junto).</summary>
internal static class EnderecoSql
{
    public const string Inserir =
        "INSERT INTO endereco (rua, cidade, bairro, numero, complemento) " +
        "VALUES (@Rua, @Cidade, @Bairro, @Numero, @Complemento)";

    public const string Atualizar =
        "UPDATE endereco SET rua = @Rua, cidade = @Cidade, bairro = @Bairro, numero = @Numero, " +
        "complemento = @Complemento WHERE idendereco = @Id";

    /// <summary>Remove o endereço só se ninguém mais o usa (evita endereço órfão).</summary>
    public const string ApagarSeOrfao =
        "DELETE FROM endereco WHERE idendereco = @enderecoId " +
        "AND NOT EXISTS (SELECT 1 FROM cliente WHERE enderecoclientefk = @enderecoId) " +
        "AND NOT EXISTS (SELECT 1 FROM funcionario WHERE enderecofuncionariofk = @enderecoId)";
}
