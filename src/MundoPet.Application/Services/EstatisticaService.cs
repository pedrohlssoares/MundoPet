using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;

namespace MundoPet.Application.Services;

public interface IEstatisticaService
{
    Task<Result<IndicadoresDto>> ObterAsync();
}

public class EstatisticaService : IEstatisticaService
{
    private readonly IEstatisticaRepository _repo;

    public EstatisticaService(IEstatisticaRepository repo) => _repo = repo;

    public async Task<Result<IndicadoresDto>> ObterAsync()
    {
        var indicadores = await _repo.ObterAsync();
        return Result<IndicadoresDto>.Ok(indicadores, "Estatísticas calculadas com sucesso.");
    }
}
