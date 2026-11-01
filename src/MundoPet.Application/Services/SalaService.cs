using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface ISalaService
{
    Task<Result<IEnumerable<SalaDto>>> ListarAsync();
    Task<Result<SalaDto>> ObterAsync(int id);
    Task<Result<SalaDto>> CriarAsync(SalaDto dto);
    Task<Result<SalaDto>> AtualizarAsync(int id, SalaDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

/// <summary>Salas mudam pouco e são consultadas toda hora na agenda. Por isso a LISTA usa cache.</summary>
public class SalaService : ISalaService
{
    private const string CacheKey = "salas:lista";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly ILogger<SalaService> _logger;
    private readonly ISalaRepository _repo;

    public SalaService(ISalaRepository repo, ICacheService cache, ILogger<SalaService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<SalaDto>>> ListarAsync()
    {
        // 1) Procura a lista no cache.
        if (_cache.TryGet<List<SalaDto>>(CacheKey, out var emCache) && emCache is not null)
        {
            _logger.LogInformation("[CACHE HIT] {Chave}: lista devolvida do cache, sem consultar o banco.", CacheKey);

            var hit = Result<IEnumerable<SalaDto>>.Ok(emCache, "Registros listados com sucesso (cache).");
            hit.FromCache = true;
            return hit;
        }

        // 2) Não estava no cache: busca no banco (fonte original) e guarda no cache.
        _logger.LogInformation("[CACHE MISS] {Chave}: lista não estava no cache, buscando no banco.", CacheKey);

        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        _cache.Set(CacheKey, lista, CacheTtl);

        var miss = Result<IEnumerable<SalaDto>>.Ok(lista, "Registros listados com sucesso.");
        miss.FromCache = false;
        return miss;
    }

    public async Task<Result<SalaDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<SalaDto>.NotFound(id);

        return Result<SalaDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<SalaDto>> CriarAsync(SalaDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<SalaDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<SalaDto>> AtualizarAsync(int id, SalaDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<SalaDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Numero = id;
        await _repo.AtualizarAsync(item);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<SalaDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static SalaDto ParaDto(Sala e) => new() { Numero = e.Numero, Tipo = e.Tipo, Descricao = e.Descricao };

    private static Sala ParaEntidade(SalaDto d) => new() { Numero = d.Numero, Tipo = d.Tipo, Descricao = d.Descricao };

    private void InvalidarCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("[CACHE INVALIDATED] {Chave}: lista removida do cache após alteração.", CacheKey);
    }
}
