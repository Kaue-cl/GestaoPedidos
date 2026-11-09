using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Mappers;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services.Interfaces;

namespace GestaoPedidos.API.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<List<ProdutoResponse>> ListarAsync()
    {
        var produtos = await _produtoRepository.ListarAsync();

        return produtos
            .Select(ProdutoMapper.ToResponse)
            .ToList();
    }

    public async Task<ProdutoResponse> ObterPorIdAsync(int id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id);

        if (produto is null)
            throw new ProdutoNaoEncontradoException(id);

        return ProdutoMapper.ToResponse(produto);
    }

    public async Task<ProdutoResponse> AdicionarAsync(
        CriarProdutoRequest request)
    {
        var produto = ProdutoMapper.ToEntity(request);

        var produtoCriado =
            await _produtoRepository.AdicionarAsync(produto);

        return ProdutoMapper.ToResponse(produtoCriado);
    }

    public async Task<ProdutoResponse> AtualizarAsync(
        int id,
        AtualizarProdutoRequest request)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id);

        if (produto is null)
            throw new ProdutoNaoEncontradoException(id);

        ProdutoMapper.UpdateEntity(request, produto);

        await _produtoRepository.AtualizarAsync(produto);

        return ProdutoMapper.ToResponse(produto);
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id);

        if (produto is null)
            throw new ProdutoNaoEncontradoException(id);

        produto.Ativo = false;

        await _produtoRepository.AtualizarAsync(produto);
    }

    public async Task<int> ConsultarEstoqueAsync(int id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id);

        if (produto is null)
            throw new ProdutoNaoEncontradoException(id);

        return produto.EstoqueAtual;
    }
}