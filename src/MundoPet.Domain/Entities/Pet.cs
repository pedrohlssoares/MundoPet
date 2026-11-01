
namespace MundoPet.Domain.Entities;

public class Pet
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public string Idade { get; set; } = string.Empty;

    /// <summary>RN001: todo pet pertence a um tutor (cliente).</summary>
    public int ClienteId { get; set; }
}
