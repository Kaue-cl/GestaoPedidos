using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.Entities;

namespace GestaoPedidos.API.Mappers;

public static class ProdutoMapper
{
    public static ProdutoResponse ToResponse(Produto produto)
    {
        return new ProdutoResponse
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            EstoqueAtual = produto.EstoqueAtual,
            Ativo = produto.Ativo
        };
    }

    public static Produto ToEntity(CriarProdutoRequest request)
    {
        return new Produto
        {
            Nome = request.Nome,
            Preco = request.Preco,
            EstoqueAtual = request.EstoqueAtual,
            Ativo = request.Ativo
        };
    }

    public static void UpdateEntity(
        AtualizarProdutoRequest request,
        Produto produto)
    {
        produto.Nome = request.Nome;
        produto.Preco = request.Preco;
        produto.EstoqueAtual = request.EstoqueAtual;
        produto.Ativo = request.Ativo
            ?? throw new ArgumentException("O status ativo é obrigatório.");
    }
}