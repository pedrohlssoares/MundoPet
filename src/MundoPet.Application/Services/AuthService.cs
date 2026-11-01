using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IAuthService
{
    Task<Result<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
}

/// <summary>Cadastro de usuários e login. A senha é guardada como hash BCrypt (nunca em texto puro).</summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ITokenService _tokens;

    public AuthService(IUsuarioRepository usuarios, ITokenService tokens)
    {
        _usuarios = usuarios;
        _tokens = tokens;
    }

    public async Task<Result<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarios.ObterPorEmailAsync(email) is not null)
        {
            return Result<UsuarioDto>.Fail(409, "E-mail já cadastrado.", "Já existe um usuário com este e-mail.");
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)   // a senha em texto puro nunca é gravada
        };

        var id = await _usuarios.AdicionarAsync(usuario);

        var criado = await _usuarios.ObterPorIdAsync(id);
        return Result<UsuarioDto>.Created(ParaDto(criado!), "Usuário cadastrado com sucesso.");
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObterPorEmailAsync(email);

        // Mesma mensagem para "usuário não existe" e "senha errada": não revela quais e-mails existem.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            return Result<AuthResponseDto>.Fail(401, "Credenciais inválidas.", "E-mail ou senha incorretos.");
        }

        var token = _tokens.Gerar(usuario);

        var resposta = new AuthResponseDto
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEmUtc,
            Usuario = ParaDto(usuario)
        };

        return Result<AuthResponseDto>.Ok(resposta, "Autenticação realizada com sucesso.");
    }

    private static UsuarioDto ParaDto(Usuario u) => new()
    {
        Id = u.Id, Nome = u.Nome, Email = u.Email, CriadoEm = u.CriadoEm
    };
}
