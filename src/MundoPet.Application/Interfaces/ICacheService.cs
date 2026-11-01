namespace MundoPet.Application.Interfaces;

/// <summary>Contrato de cache. A implementação (memória) fica na Infrastructure.</summary>
public interface ICacheService
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan ttl);
    void Remove(string key);
}
