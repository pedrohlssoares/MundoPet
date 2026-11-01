using Microsoft.Extensions.Logging;
using MundoPet.Application.Common;
using MundoPet.Application.Dtos;
using MundoPet.Application.Interfaces;
using MundoPet.Domain.Entities;
using MundoPet.Domain.Interfaces;

namespace MundoPet.Application.Services;

public interface IPagamentoService
{
    Task<Result<IEnumerable<PagamentoDto>>> ListarAsync();
    Task<Result<PagamentoDto>> ObterAsync(int id);
    Task<Result<PagamentoDto>> CriarAsync(PagamentoDto dto);
    Task<Result<PagamentoDto>> AtualizarAsync(int id, PagamentoDto dto);
    Task<Result<object?>> ExcluirAsync(int id);
}

public class PagamentoService : IPagamentoService
{
    private readonly IPagamentoRepository _repo;

    public PagamentoService(IPagamentoRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<PagamentoDto>>> ListarAsync()
    {
        var lista = (await _repo.ObterTodosAsync()).Select(ParaDto).ToList();
        return Result<IEnumerable<PagamentoDto>>.Ok(lista, "Registros listados com sucesso.");
    }

    public async Task<Result<PagamentoDto>> ObterAsync(int id)
    {
        var item = await _repo.ObterPorIdAsync(id);
        if (item is null) return Result<PagamentoDto>.NotFound(id);

        return Result<PagamentoDto>.Ok(ParaDto(item), "Registro encontrado.");
    }

    public async Task<Result<PagamentoDto>> CriarAsync(PagamentoDto dto)
    {
        var item = ParaEntidade(dto);
        var id = await _repo.AdicionarAsync(item);

        var criado = await _repo.ObterPorIdAsync(id);
        return Result<PagamentoDto>.Created(ParaDto(criado!));
    }

    public async Task<Result<PagamentoDto>> AtualizarAsync(int id, PagamentoDto dto)
    {
        if (await _repo.ObterPorIdAsync(id) is null) return Result<PagamentoDto>.NotFound(id);

        var item = ParaEntidade(dto);
        item.Id = id;
        await _repo.AtualizarAsync(item);

        var atualizado = await _repo.ObterPorIdAsync(id);
        return Result<PagamentoDto>.Ok(ParaDto(atualizado!), "Registro atualizado com sucesso.");
    }

    public async Task<Result<object?>> ExcluirAsync(int id)
    {
        if (!await _repo.RemoverAsync(id)) return Result<object?>.NotFound(id);

        return Result<object?>.Ok(null, "Registro excluído com sucesso.");
    }

    private static PagamentoDto ParaDto(Pagamento e) => new() { Id = e.Id, Prestacoes = e.Prestacoes, Valor = e.Valor, DataPagamento = e.DataPagamento, FormaPagamentoId = e.FormaPagamentoId, ClienteId = e.ClienteId, ConsultaId = e.ConsultaId, PrescricaoId = e.PrescricaoId, VendaId = e.VendaId };

    private static Pagamento ParaEntidade(PagamentoDto d) => new() { Id = d.Id, Prestacoes = d.Prestacoes, Valor = d.Valor, DataPagamento = d.DataPagamento, FormaPagamentoId = d.FormaPagamentoId, ClienteId = d.ClienteId, ConsultaId = d.ConsultaId, PrescricaoId = d.PrescricaoId, VendaId = d.VendaId };
}
