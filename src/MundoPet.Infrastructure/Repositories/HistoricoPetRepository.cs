using Dapper;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Infrastructure.Data;

namespace MundoPet.Infrastructure.Repositories;

/// <summary>Leituras do painel do pet (prontuário): junta pet, tutor, consultas, vacinas e vendas.</summary>
public class HistoricoPetRepository : IHistoricoPetRepository
{
    private readonly DbConnectionFactory _db;

    public HistoricoPetRepository(DbConnectionFactory db) => _db = db;

    public async Task<HistoricoPetDto?> ObterAsync(int idPet)
    {
        using var con = _db.CriarConexao();

        var pet = await con.QuerySingleOrDefaultAsync<PetTutorDto>(
            "SELECT p.idpet AS Id, p.nome AS Nome, p.especie AS Especie, p.raca AS Raca, p.idade AS Idade, " +
            "c.nome AS TutorNome, c.cpf AS TutorCpf, c.celular AS TutorCelular " +
            "FROM pet p INNER JOIN cliente c ON p.clientepetfk = c.idcliente " +
            "WHERE p.idpet = @idPet",
            new { idPet });

        if (pet is null) return null;

        var consultas = await con.QueryAsync<ConsultaHistoricoDto>(
            "SELECT c.idconsulta AS Id, c.data_consulta AS DataConsulta, c.horario AS Horario, " +
            "c.status AS Status, c.processos_feitos AS ProcessosFeitos, " +
            "s.numero AS SalaNumero, s.tipo AS SalaTipo " +
            "FROM consulta c LEFT JOIN sala s ON c.salaconsultafk = s.numero " +
            "WHERE c.petconsultafk = @idPet " +
            "ORDER BY c.data_consulta DESC, c.horario DESC",
            new { idPet });

        var vacinas = await con.QueryAsync<VacinaHistoricoDto>(
            "SELECT hv.idhistorico AS Id, hv.data_aplicacao AS DataAplicacao, hv.dosagem AS Dosagem, " +
            "v.ativo AS Ativo, pr.nome AS Produto " +
            "FROM historico_vacinacao hv " +
            "INNER JOIN vacina v ON hv.vacinahistorico_vacinacaofk = v.idvacina " +
            "INNER JOIN produto pr ON v.produtovacinafk = pr.idproduto " +
            "WHERE hv.pethistorico_vacinacaofk = @idPet " +
            "ORDER BY hv.data_aplicacao DESC",
            new { idPet });

        // O vínculo real é pagamento.vendapagamentofk -> venda e pagamento.consultapagamentofk -> consulta.
        var vendas = await con.QueryAsync<VendaHistoricoDto>(
            "SELECT v.idvenda AS VendaId, pr.nome AS Produto, v.quantidade AS Quantidade, " +
            "v.valor_unitario AS ValorUnitario, pag.data_pagamento AS DataPagamento " +
            "FROM venda v " +
            "INNER JOIN produto pr ON v.produtovendafk = pr.idproduto " +
            "INNER JOIN pagamento pag ON pag.vendapagamentofk = v.idvenda " +
            "INNER JOIN consulta c ON pag.consultapagamentofk = c.idconsulta " +
            "WHERE c.petconsultafk = @idPet " +
            "ORDER BY pag.data_pagamento DESC",
            new { idPet });

        var totalGasto = await con.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(pag.valor), 0) FROM pagamento pag " +
            "INNER JOIN consulta c ON pag.consultapagamentofk = c.idconsulta " +
            "WHERE c.petconsultafk = @idPet",
            new { idPet });

        return new HistoricoPetDto
        {
            Pet = pet,
            Consultas = consultas.ToList(),
            Vacinas = vacinas.ToList(),
            Vendas = vendas.ToList(),
            TotalGasto = totalGasto
        };
    }
}
