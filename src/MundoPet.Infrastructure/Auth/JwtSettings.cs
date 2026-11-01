namespace MundoPet.Infrastructure.Auth;

/// <summary>Configurações do JWT, lidas do appsettings (seção "Jwt"). A chave não fica fixa no código.</summary>
public class JwtSettings
{
    /// <summary>Chave secreta usada para assinar o token (HMAC-SHA256). Mínimo de 32 caracteres.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MundoPet.Api";
    public string Audience { get; set; } = "MundoPet.Client";
    public int ExpiresMinutes { get; set; } = 60;
}
