
namespace MundoPet.Domain.Entities;

public class Prescricao
{
    public int Id { get; set; }
    public int ConsultaId { get; set; }
    public int? RemedioId { get; set; }
    public int? VacinaId { get; set; }
    public string Dosagem { get; set; } = string.Empty;
}
