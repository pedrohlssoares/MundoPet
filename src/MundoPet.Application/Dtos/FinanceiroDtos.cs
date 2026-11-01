using System.ComponentModel.DataAnnotations;

namespace MundoPet.Application.Dtos;

public class ProdutoDto
{
    public int Id { get; set; }

    [Required, StringLength(45)]
    public string Nome { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantidade { get; set; }

    [Required, StringLength(400)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0, 999999.99)]
    public decimal Preco { get; set; }
}

public class VendaDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o produto.")]
    public int ProdutoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    [Range(0.01, 999999.99)]
    public decimal ValorUnitario { get; set; }
}

public class FormaPagamentoDto
{
    public int Id { get; set; }

    [Required, StringLength(45)]
    public string Tipo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Descricao { get; set; } = string.Empty;
}

public class PagamentoDto
{
    public int Id { get; set; }

    [Range(1, 1000)]
    public int Prestacoes { get; set; } = 1;

    [Range(0.01, 999999.99)]
    public decimal Valor { get; set; }

    /// <summary>Opcional. Se omitida, usa a data/hora atual.</summary>
    public DateTime? DataPagamento { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a forma de pagamento.")]
    public int FormaPagamentoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o cliente.")]
    public int ClienteId { get; set; }

    public int? ConsultaId { get; set; }
    public int? PrescricaoId { get; set; }
    public int? VendaId { get; set; }
}

public class DespesaDto
{
    public int Id { get; set; }

    [Range(0.01, 999999.99)]
    public decimal Preco { get; set; }

    /// <summary>Formato aaaa-mm-dd.</summary>
    [Required(ErrorMessage = "Informe a data da despesa.")]
    public DateTime? Data { get; set; }

    [Required, StringLength(400)]
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>Painel de estatísticas gerais.</summary>
public class IndicadoresDto
{
    public decimal TotalFaturado { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal BalancoLiquido => TotalFaturado - TotalDespesas;

    public decimal FaturamentoVendas { get; set; }
    public long QtdItensVendidos { get; set; }

    public long TotalProdutos { get; set; }
    public long EstoqueTotalProdutos { get; set; }

    public long TotalClientes { get; set; }
    public long TotalPets { get; set; }
    public long TotalConsultas { get; set; }
    public long TotalRemedios { get; set; }
    public long TotalVacinas { get; set; }
    public long TotalAplicacoesVacina { get; set; }
}
