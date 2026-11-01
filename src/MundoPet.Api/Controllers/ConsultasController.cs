using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPet.Application.Dtos;
using MundoPet.Application.Services;

namespace MundoPet.Api.Controllers;

/// <summary>Só recebe a requisição HTTP, chama o service e devolve o Result (ResultWrapper). Sem regra de negócio aqui.</summary>
[ApiController]
[Authorize]   // exige token JWT válido
[Route("api/consultas")]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service) => _service = service;

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
    public async Task<IActionResult> Criar([FromBody] ConsultaDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return StatusCode(resultado.StatusCode, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ConsultaDto dto)
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

    /// <summary>Agenda de um dia. Ex.: GET /api/consultas/por-data?data=2026-06-04</summary>
    [HttpGet("por-data")]
    public async Task<IActionResult> ListarPorData([FromQuery] DateTime? data)
    {
        var resultado = await _service.ListarPorDataAsync(data);
        return StatusCode(resultado.StatusCode, resultado);
    }

    /// <summary>"Status rápido" da agenda: Agendada, Realizada ou Cancelada.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] AlterarStatusDto dto)
    {
        var resultado = await _service.AlterarStatusAsync(id, dto.Status);
        return StatusCode(resultado.StatusCode, resultado);
    }
}
