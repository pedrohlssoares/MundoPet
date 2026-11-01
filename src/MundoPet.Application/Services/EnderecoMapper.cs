using MundoPet.Application.Dtos;
using MundoPet.Domain.Entities;

namespace MundoPet.Application.Services;

/// <summary>Converte Endereco ⇄ EnderecoDto (usado por Cliente e Funcionário).</summary>
internal static class EnderecoMapper
{
    public static EnderecoDto ParaDto(Endereco e) => new()
    {
        Id = e.Id, Rua = e.Rua, Cidade = e.Cidade, Bairro = e.Bairro, Numero = e.Numero, Complemento = e.Complemento
    };

    public static Endereco ParaEntidade(EnderecoDto d) => new()
    {
        Id = d.Id, Rua = d.Rua, Cidade = d.Cidade, Bairro = d.Bairro, Numero = d.Numero, Complemento = d.Complemento
    };
}
