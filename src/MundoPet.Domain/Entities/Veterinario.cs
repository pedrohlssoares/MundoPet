
namespace MundoPet.Domain.Entities;

public class Veterinario
{
    public int Id { get; set; }
    public string Crmv { get; set; } = string.Empty;
    public int FuncionarioId { get; set; }
    public string? Descricao { get; set; }

    /// <summary>Somente leitura: nome do funcionário vinculado (preenchido por JOIN na consulta).</summary>
    public string? Nome { get; set; }
}
