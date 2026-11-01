using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;

namespace MundoPet.Infrastructure.Auth;

/// <summary>
/// Gera o JWT (header.payload.assinatura). O payload leva as "claims" do usuário:
///   sub = id, email, name = nome. A API valida assinatura, emissor, audiência e expiração
///   em cada requisição a um endpoint protegido com [Authorize].
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings) => _settings = settings;

    public TokenInfo Gerar(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("name", usuario.Nome)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciais);

        return new TokenInfo(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
