using MundoPet.Domain.Common;

namespace MundoPet.Domain.Entities;

public class Consulta
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VeterinarioId { get; set; }
    /// <summary>Número da sala (sala.numero).</summary>
    public int SalaId { get; set; }
    public DateTime DataConsulta { get; set; }
    public TimeSpan Horario { get; set; }
    public string Status { get; set; } = StatusConsulta.Agendada;
    public string ProcessosFeitos { get; set; } = string.Empty;
}
