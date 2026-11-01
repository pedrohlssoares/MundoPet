using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPet.Application.Dtos;
using MundoPet.Application.Services;

namespace MundoPet.Api.Controllers;

/// <summary>Só recebe a requisição HTTP, chama o service e devolve o Result (ResultWrapper). Sem regra de negócio aqui.</summary>
[ApiController]
[Authorize]   // exige token JWT válido
[Route("api/prescricoes")]
public class PrescricoesController : ControllerBase
{
    private readonly IPrescricaoService _service;

    public PrescricoesController(IPrescricaoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _service.ListarAsync();
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var resultado = await _service.ObterAsync(id);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PrescricaoDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PrescricaoDto dto)
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
