namespace MundoPet.Domain.Entities;

/// <summary>Usuário do sistema. A senha nunca é guardada em texto puro: apenas o hash (BCrypt).</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
