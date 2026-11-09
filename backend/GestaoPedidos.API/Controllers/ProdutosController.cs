using GestaoPedidos.API.DTOs.Produtos;
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
        var produto = await _produtoService.ObterPorIdAsync(id);
        return Ok(produto);
    }

    [HttpGet("{id:int}/estoque")]
    public async Task<IActionResult> ConsultarEstoque(int id)
    {
        var estoque = await _produtoService.ConsultarEstoqueAsync(id);

        return Ok(new
        {
            produtoId = id,
            estoqueAtual = estoque
        });
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
        var produto = await _produtoService.AtualizarAsync(id, request);
        return Ok(produto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _produtoService.ExcluirAsync(id);
        return NoContent();
    }

    [HttpGet("paginado")]
    public async Task<IActionResult> ListarPaginado(
        [FromQuery] FiltroProdutosRequest filtro)
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

        if (!new[] { "nome", "preco", "estoque" }.Contains(
                filtro.OrdenarPor.ToLowerInvariant()) ||
            !new[] { "asc", "desc" }.Contains(
                filtro.Direcao.ToLowerInvariant()))
        {
            return BadRequest(new
            {
                mensagem = "Parâmetros de ordenação inválidos."
            });
        }

        var resultado = await _produtoService.ListarPaginadoAsync(filtro);
        return Ok(resultado);
    }
}