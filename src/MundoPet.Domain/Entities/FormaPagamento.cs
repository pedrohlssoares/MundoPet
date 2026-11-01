
namespace MundoPet.Domain.Entities;

public class FormaPagamento
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
