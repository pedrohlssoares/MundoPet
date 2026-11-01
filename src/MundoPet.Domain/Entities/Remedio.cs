
namespace MundoPet.Domain.Entities;

public class Remedio
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string Ativo { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
}
