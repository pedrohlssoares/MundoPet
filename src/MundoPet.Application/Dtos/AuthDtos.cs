using System.ComponentModel.DataAnnotations;

namespace MundoPet.Application.Dtos;

public class RegistrarUsuarioDto
{
    [Required, StringLength(90)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(60)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    /// <summary>Data/hora (UTC) em que o token expira.</summary>
    public DateTime ExpiraEm { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}
