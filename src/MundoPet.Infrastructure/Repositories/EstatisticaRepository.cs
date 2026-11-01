using Dapper;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

/// <summary>Todos os indicadores do painel em uma única ida ao banco.</summary>
public class EstatisticaRepository : IEstatisticaRepository
{
    private const string Sql =
        "SELECT " +
        "COALESCE((SELECT SUM(valor) FROM pagamento), 0) AS TotalFaturado, " +
        "COALESCE((SELECT SUM(preco) FROM despesa), 0) AS TotalDespesas, " +
        "COALESCE((SELECT SUM(quantidade * valor_unitario) FROM venda), 0) AS FaturamentoVendas, " +
        "CAST(COALESCE((SELECT SUM(quantidade) FROM venda), 0) AS SIGNED) AS QtdItensVendidos, " +
        "(SELECT COUNT(*) FROM produto) AS TotalProdutos, " +
        "CAST(COALESCE((SELECT SUM(quantidade) FROM produto), 0) AS SIGNED) AS EstoqueTotalProdutos, " +
        "(SELECT COUNT(*) FROM cliente) AS TotalClientes, " +
        "(SELECT COUNT(*) FROM pet) AS TotalPets, " +
        "(SELECT COUNT(*) FROM consulta) AS TotalConsultas, " +
        "(SELECT COUNT(*) FROM remedio) AS TotalRemedios, " +
        "(SELECT COUNT(*) FROM vacina) AS TotalVacinas, " +
        "(SELECT COUNT(*) FROM historico_vacinacao) AS TotalAplicacoesVacina";

    private readonly DbConnectionFactory _db;

    public EstatisticaRepository(DbConnectionFactory db) => _db = db;

    public async Task<IndicadoresDto> ObterAsync()
    {
        using var con = _db.CriarConexao();
        return await con.QuerySingleAsync<IndicadoresDto>(Sql);
    }
}
