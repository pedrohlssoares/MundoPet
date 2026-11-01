using MundoPet.Domain.Entities;

namespace MundoPet.Application.Interfaces;

public record TokenInfo(string Token, DateTime ExpiraEmUtc);

public interface ITokenService
{
    TokenInfo Gerar(Usuario usuario);
}
