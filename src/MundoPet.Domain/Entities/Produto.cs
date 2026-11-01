
namespace MundoPet.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
