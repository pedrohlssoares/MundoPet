using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MundoPet.Application.Services;

namespace MundoPet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/estatisticas")]
public class EstatisticasController : ControllerBase
{
    private readonly IEstatisticaService _service;

    public EstatisticasController(IEstatisticaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var resultado = await _service.ObterAsync();
        return StatusCode(resultado.StatusCode, resultado);
    }
}
