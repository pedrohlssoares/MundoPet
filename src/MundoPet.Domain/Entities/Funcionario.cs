
namespace MundoPet.Domain.Entities;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public int EnderecoId { get; set; }
    public Endereco Endereco { get; set; } = new();
}
