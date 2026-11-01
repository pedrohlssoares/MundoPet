using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Services;

namespace MundoPet.Api.Controllers;

/// <summary>Cadastro de usuário e login. Estes endpoints são públicos; o restante da API exige token.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    /// <summary>Valida e-mail e senha e devolve o token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _service.LoginAsync(dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    /// <summary>Cadastro de um novo usuário.</summary>
    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto)
    {
        var resultado = await _service.RegistrarAsync(dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    /// <summary>Dados do usuário dono do token (exemplo de endpoint protegido).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        _ = int.TryParse(User.FindFirst("sub")?.Value, out var id);

        var usuario = new UsuarioDto
        {
            Id = id,
            Nome = User.FindFirst("name")?.Value ?? string.Empty,
            Email = User.FindFirst("email")?.Value ?? string.Empty
        };

        return Ok(Result<UsuarioDto>.Ok(usuario, "Token válido."));
    }
}
