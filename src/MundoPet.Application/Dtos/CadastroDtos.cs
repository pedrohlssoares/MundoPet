using System.ComponentModel.DataAnnotations;

namespace MundoPet.Application.Dtos;

public class EnderecoDto
{
    public int Id { get; set; }

    [Required, StringLength(90)]
    public string Rua { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Cidade { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Bairro { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Numero { get; set; } = string.Empty;

    [StringLength(45)]
    public string? Complemento { get; set; }
}

public class ClienteDto
{
    public int Id { get; set; }

    [Required, StringLength(90)]
    public string Nome { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve ter 11 dígitos numéricos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(60)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{10,11}$", ErrorMessage = "O celular deve ter 10 ou 11 dígitos numéricos.")]
    public string Celular { get; set; } = string.Empty;

    /// <summary>Somente leitura.</summary>
    public int EnderecoId { get; set; }

    [Required]
    public EnderecoDto Endereco { get; set; } = new();
}

public class FuncionarioDto
{
    public int Id { get; set; }

    [Required, StringLength(90)]
    public string Nome { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{10,11}$", ErrorMessage = "O celular deve ter 10 ou 11 dígitos numéricos.")]
    public string Celular { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve ter 11 dígitos numéricos.")]
    public string Cpf { get; set; } = string.Empty;

    [Range(0, 999999.99)]
    public decimal Salario { get; set; }

    /// <summary>Somente leitura.</summary>
    public int EnderecoId { get; set; }

    [Required]
    public EnderecoDto Endereco { get; set; } = new();
}

public class PetDto
{
    public int Id { get; set; }

    [Required, StringLength(45)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Especie { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Raca { get; set; } = string.Empty;

    /// <summary>Texto livre, ex.: "3 anos".</summary>
    [Required, StringLength(45)]
    public string Idade { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Informe o cliente (tutor) do pet.")]
    public int ClienteId { get; set; }
}

public class SalaDto
{
    /// <summary>Número da sala (chave primária).</summary>
    public int Numero { get; set; }

    [Required, StringLength(45)]
    public string Tipo { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string Descricao { get; set; } = string.Empty;
}

public class VeterinarioDto
{
    public int Id { get; set; }

    [Required, StringLength(11)]
    public string Crmv { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Informe o funcionário.")]
    public int FuncionarioId { get; set; }

    [StringLength(400)]
    public string? Descricao { get; set; }

    /// <summary>Somente leitura: nome do funcionário.</summary>
    public string? Nome { get; set; }
}
