using GestaoPedidos.API.DTOs.Pedidos;
using GestaoPedidos.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.API.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Criar(
        [FromBody] CriarPedidoRequest request)
    {
        var pedido = await _pedidoService.CriarAsync(request);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = pedido.Id },
            pedido);
    }

    
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltroPedidosRequest filtro)
    {
        var pedidos = await _pedidoService.ListarAsync(filtro);

        var totalRegistros = await _pedidoService.ContarAsync(filtro);

        return Ok(new
        {
            pagina = filtro.Pagina,
            tamanhoPagina = filtro.TamanhoPagina,
            totalRegistros,
            totalPaginas = (int)Math.Ceiling(
                (double)totalRegistros / filtro.TamanhoPagina),
            dados = pedidos
        });
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoResponse>> ObterPorId(
        int id)
    {
        var pedido = await _pedidoService.ObterPorIdAsync(id);

        return Ok(pedido);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<PedidoResponse>> AtualizarStatus(
        int id,
        [FromBody] AtualizarStatusPedidoRequest request)
    {
        var pedido = await _pedidoService
            .AtualizarStatusAsync(id, request);

        return Ok(pedido);
    }

    [HttpPatch("{id:int}/cancelar")]
    public async Task<ActionResult<PedidoResponse>> Cancelar(
        int id)
    {
        var pedido = await _pedidoService.CancelarAsync(id);

        return Ok(pedido);
    }
}