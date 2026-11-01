
namespace MundoPet.Domain.Entities;

public class HistoricoVacinacao
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VacinaId { get; set; }
    public DateTime DataAplicacao { get; set; }
    public string Dosagem { get; set; } = string.Empty;
}
