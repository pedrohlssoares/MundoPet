using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IProdutoService
{
    Task<Result<IEnumerable<ProdutoDto>>> ListarAsync();
    Task<Result<ProdutoDto>> ObterAsync(int id);
    Task<Result<ProdutoDto>> CriarAsync(ProdutoDto dto);
    Task<Result<ProdutoDto>> AtualizarAsync(int id, ProdutoDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

/// <summary>Catálogo de produtos: lido o tempo todo (vendas, remédios, vacinas) e alterado raramente. Por isso a LISTA usa cache.</summary>
public class ProdutoService : IProdutoService
{
    private const string CacheKey = "produtos:lista";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly ILogger<ProdutoService> _logger;
    private readonly IProdutoRepository _repo;

    public ProdutoService(IProdutoRepository repo, ICacheService cache, ILogger<ProdutoService> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<ProdutoDto>>> ListarAsync()
    {
        // 1) Procura a lista no cache.
        if (_cache.TryGet<List<ProdutoDto>>(CacheKey, out var emCache) && emCache is not null)
        {
            _logger.LogInformation("[CACHE HIT] {Chave}: lista devolvida do cache, sem consultar o banco.", CacheKey);

            var hit = Result<IEnumerable<ProdutoDto>>.Ok(emCache, "Registros listados com sucesso (cache).");
            hit.FromCache = true;
            return hit;
        }

        // 2) Não estava no cache: busca no banco (fonte original) e guarda no cache.
        _logger.LogInformation("[CACHE MISS] {Chave}: lista não estava no cache, buscando no banco.", CacheKey);

        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        _cache.Set(CacheKey, lista, CacheTtl);

        var miss = Result<IEnumerable<ProdutoDto>>.Ok(lista, "Registros listados com sucesso.");
        miss.FromCache = false;
        return miss;
    }

    public async Task<Result<ProdutoDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<ProdutoDto>.NotFound(id);

        return Result<ProdutoDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<ProdutoDto>> CriarAsync(ProdutoDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<ProdutoDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<ProdutoDto>> AtualizarAsync(int id, ProdutoDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<ProdutoDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<ProdutoDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        // 3) Houve alteração: o cache ficou desatualizado, então é removido.
        InvalidarCache();

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static ProdutoDto ParaDto(Produto e) => new() { Id = e.Id, Nome = e.Nome, Quantidade = e.Quantidade, Descricao = e.Descricao, Preco = e.Preco };

    private static Produto ParaEntidade(ProdutoDto d) => new() { Id = d.Id, Nome = d.Nome, Quantidade = d.Quantidade, Descricao = d.Descricao, Preco = d.Preco };

    private void InvalidarCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("[CACHE INVALIDATED] {Chave}: lista removida do cache após alteração.", CacheKey);
    }
}
