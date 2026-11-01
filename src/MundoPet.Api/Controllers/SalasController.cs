using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPet.Application.Dtos;
using MundoPet.Application.Services;

namespace MundoPet.Api.Controllers;

/// <summary>Só recebe a requisição HTTP, chama o service e devolve o Result (ResultWrapper). Sem regra de negócio aqui.</summary>
[ApiController]
[Authorize]   // exige token JWT válido
[Route("api/salas")]
public class SalasController : ControllerBase
{
    private readonly ISalaService _service;

    public SalasController(ISalaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();

        // Mostra no Swagger/navegador se a lista veio do cache (HIT) ou do banco (MISS).
        if (resultado.FromCache is { } doCache)
        {
            Response.Headers["X-Cache"] = doCache ? "HIT" : "MISS";
        }

        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var resultado = await _service.ObterAsync(id);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SalaDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] SalaDto dto)
    {
        var resultado = await _service.AtualizarAsync(id, dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _service.ExcluirAsync(id);
        return StatusCode(resultado.StatusCode, resultado);
    }
}
