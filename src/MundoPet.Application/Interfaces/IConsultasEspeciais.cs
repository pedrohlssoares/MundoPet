using MundoPet.Application.Dtos;

namespace MundoPet.Application.Interfaces;

// Leituras que juntam várias tabelas e devolvem DTOs (não pertencem a uma entidade só).

public interface IEstatisticaRepository
{
    Task<IndicadoresDto> ObterAsync();
}

public interface IHistoricoPetRepository
{
    Task<HistoricoPetDto?> ObterAsync(int idPet);
}
