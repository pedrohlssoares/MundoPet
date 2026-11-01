namespace MundoPet.Domain.Entities;

public class Sala
{
    /// <summary>Número da sala: é a chave primária da tabela.</summary>
    public int Numero { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
