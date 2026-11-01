
namespace MundoPet.Domain.Entities;

/// <summary>Tutor do pet. O endereço fica em outra tabela (endereco).</summary>
public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public int EnderecoId { get; set; }
    public Endereco Endereco { get; set; } = new();
}
