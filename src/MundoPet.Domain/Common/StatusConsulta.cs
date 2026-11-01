namespace MundoPet.Domain.Common;

public static class StatusConsulta
{
    public const string Agendada = "Agendada";
    public const string Realizada = "Realizada";
    public const string Cancelada = "Cancelada";

    /// <summary>Expressão regular usada para validar o status nos DTOs.</summary>
    public const string Padrao = "^(Agendada|Realizada|Cancelada)$";
}
