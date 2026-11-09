using GestaoPedidos.API.DTOs.Dashboard;
using GestaoPedidos.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(DashboardResponse),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> ObterIndicadores()
    {
        var resultado = await _service.ObterIndicadoresAsync();

        return Ok(resultado);
    }
}