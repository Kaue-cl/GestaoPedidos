using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestaoPedidos.API.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var produtos = await _produtoService.ListarAsync();

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var produto = await _produtoService.ObterPorIdAsync(id);

            return Ok(produto);
        }
        catch (ProdutoNaoEncontradoException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{id:int}/estoque")]
    public async Task<IActionResult> ConsultarEstoque(int id)
    {
        try
        {
            var estoque = await _produtoService.ConsultarEstoqueAsync(id);

            return Ok(new
            {
                ProdutoId = id,
                EstoqueAtual = estoque
            });
        }
        catch (ProdutoNaoEncontradoException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(
        [FromBody] CriarProdutoRequest request)
    {
        var produto = await _produtoService.AdicionarAsync(request);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = produto.Id },
            produto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] AtualizarProdutoRequest request)
    {
        try
        {
            var produto = await _produtoService.AtualizarAsync(
                id,
                request);

            return Ok(produto);
        }
        catch (ProdutoNaoEncontradoException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _produtoService.ExcluirAsync(id);

            return NoContent();
        }
        catch (ProdutoNaoEncontradoException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }
}