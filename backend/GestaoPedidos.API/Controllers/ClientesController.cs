using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GestaoPedidos.API.Exceptions;

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
    [ProducesResponseType(
        typeof(List<ClienteResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClienteResponse>>> Listar()
    {
        var clientes = await _clienteService.ListarAsync();
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ClienteResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(int id)
    {
        try
        {
            var cliente = await _clienteService.ObterPorIdAsync(id);
            return Ok(cliente);
        }
        catch (ClienteNaoEncontradoException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ClienteResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Adicionar(
        [FromBody] CriarClienteRequest request)
    {
        try
        {
            var cliente = await _clienteService.AdicionarAsync(request);
            
            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = cliente.Id },
                cliente
            );
        }
        catch (EmailJaCadastradoException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ClienteResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Atualizar(
        int id,
        [FromBody] AtualizarClienteRequest request)
    {
        try
        {
            var cliente = await _clienteService.AtualizarAsync(id, request);
            return Ok(cliente);
        }
        catch (ClienteNaoEncontradoException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (EmailJaCadastradoException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _clienteService.ExcluirAsync(id);
            return NoContent();
        }
        catch (ClienteNaoEncontradoException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}

