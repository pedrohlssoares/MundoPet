
namespace MundoPet.Domain.Entities;

public class Despesa
{
    public int Id { get; set; }
    public decimal Preco { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
