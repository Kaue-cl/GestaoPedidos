using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.API.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var clientes = await _clienteService.ListarAsync();
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var cliente = await _clienteService.ObterPorIdAsync(id);
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(
        [FromBody] CriarClienteRequest request)
    {
        var cliente = await _clienteService.AdicionarAsync(request);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = cliente.Id },
            cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] AtualizarClienteRequest request)
    {
        var cliente = await _clienteService.AtualizarAsync(id, request);
        return Ok(cliente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _clienteService.ExcluirAsync(id);
        return NoContent();
    }

    [HttpGet("paginado")]
    public async Task<IActionResult> ListarPaginado(
        [FromQuery] FiltroClientesRequest filtro)
    {
        if (filtro.Pagina < 1 ||
            filtro.TamanhoPagina < 1 ||
            filtro.TamanhoPagina > 100)
        {
            return BadRequest(new
            {
                mensagem = "Parâmetros de paginação inválidos."
            });
        }

        if (!new[] { "nome", "data" }.Contains(
                filtro.OrdenarPor.ToLowerInvariant()) ||
            !new[] { "asc", "desc" }.Contains(
                filtro.Direcao.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                mensagem = "Parâmetros de ordenação inválidos."
            });
        }

        var resultado = await _clienteService.ListarPaginadoAsync(filtro);
        return Ok(resultado);
    }
}