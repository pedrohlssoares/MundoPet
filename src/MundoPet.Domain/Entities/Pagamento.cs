
namespace MundoPet.Domain.Entities;

public class Pagamento
{
    public int Id { get; set; }
    public int Prestacoes { get; set; } = 1;
    public decimal Valor { get; set; }
    /// <summary>Se nulo, o banco registra a data/hora atual.</summary>
    public DateTime? DataPagamento { get; set; }
    public int FormaPagamentoId { get; set; }
    public int ClienteId { get; set; }
    public int? ConsultaId { get; set; }
    public int? PrescricaoId { get; set; }
    public int? VendaId { get; set; }
}
