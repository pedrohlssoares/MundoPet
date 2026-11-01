using Microsoft.Extensions.Caching.Memory;
using MundoPet.Application.Interfaces;

namespace MundoPet.Infrastructure.Cache;

/// <summary>
/// Cache em memória do processo (IMemoryCache). Simples e rápido; os dados somem quando a API reinicia.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan ttl) => _cache.Set(key, value, ttl);

    public void Remove(string key) => _cache.Remove(key);
}
