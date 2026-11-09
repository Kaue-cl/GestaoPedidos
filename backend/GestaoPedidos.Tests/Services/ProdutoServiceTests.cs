using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services;
using Moq;

namespace GestaoPedidos.Tests.Services;

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _repositoryMock;
    private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        _repositoryMock = new Mock<IProdutoRepository>();

        _service = new ProdutoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task ObterPorId_ProdutoExistente_RetornaProduto()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Notebook Dell",
            Preco = 3500m,
            EstoqueAtual = 10,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(produto);

        // Act
        var resultado = await _service.ObterPorIdAsync(1);

        // Assert
        Assert.Equal(produto.Id, resultado.Id);
        Assert.Equal(produto.Nome, resultado.Nome);
        Assert.Equal(produto.Preco, resultado.Preco);
        Assert.Equal(produto.EstoqueAtual, resultado.EstoqueAtual);
        Assert.Equal(produto.Ativo, resultado.Ativo);
    }

    [Fact]
    public async Task ObterPorId_ProdutoInexistente_LancaExcecao()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Produto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(
            () => _service.ObterPorIdAsync(999));
    }

    [Fact]
    public async Task Adicionar_DadosValidos_CadastraProduto()
    {
        // Arrange
        var request = new CriarProdutoRequest
        {
            Nome = "Mouse Logitech",
            Preco = 150m,
            EstoqueAtual = 20,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Produto>()))
            .ReturnsAsync((Produto produto) =>
            {
                produto.Id = 1;
                return produto;
            });

        // Act
        var resultado = await _service.AdicionarAsync(request);

        // Assert
        Assert.Equal(1, resultado.Id);
        Assert.Equal(request.Nome, resultado.Nome);
        Assert.Equal(request.Preco, resultado.Preco);
        Assert.Equal(request.EstoqueAtual, resultado.EstoqueAtual);
        Assert.True(resultado.Ativo);

        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Produto>()),
            Times.Once);
    }

    [Fact]
    public async Task Atualizar_ProdutoExistente_AtualizaDados()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Mouse Antigo",
            Preco = 100m,
            EstoqueAtual = 5,
            Ativo = true
        };

        var request = new AtualizarProdutoRequest
        {
            Nome = "Mouse Atualizado",
            Preco = 200m,
            EstoqueAtual = 15,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(produto);

        // Act
        var resultado = await _service.AtualizarAsync(1, request);

        // Assert
        Assert.Equal(request.Nome, resultado.Nome);
        Assert.Equal(request.Preco, resultado.Preco);
        Assert.Equal(request.EstoqueAtual, resultado.EstoqueAtual);
        Assert.True(resultado.Ativo);

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.Is<Produto>(p =>
                p.Id == 1 &&
                p.Nome == request.Nome &&
                p.Preco == request.Preco &&
                p.EstoqueAtual == request.EstoqueAtual)),
            Times.Once);
    }

    [Fact]
    public async Task Excluir_ProdutoExistente_RealizaExclusaoLogica()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Teclado Mecânico",
            Preco = 250m,
            EstoqueAtual = 8,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(produto);

        // Act
        await _service.ExcluirAsync(1);

        // Assert
        Assert.False(produto.Ativo);

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.Is<Produto>(p =>
                p.Id == 1 && !p.Ativo)),
            Times.Once);

        _repositoryMock.Verify(
            r => r.ExcluirAsync(It.IsAny<Produto>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsultarEstoque_ProdutoExistente_RetornaQuantidade()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Monitor 24 polegadas",
            Preco = 900m,
            EstoqueAtual = 12,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(produto);

        // Act
        var estoque = await _service.ConsultarEstoqueAsync(1);

        // Assert
        Assert.Equal(12, estoque);
    }

    [Fact]
    public async Task Atualizar_ProdutoInexistente_LancaExcecao()
    {
        var request = new AtualizarProdutoRequest
        {
            Nome = "Produto Atualizado",
            Preco = 100m,
            EstoqueAtual = 10,
            Ativo = true
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(
            () => _service.AtualizarAsync(999, request));

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Produto>()),
            Times.Never);
    }

    [Fact]
    public async Task Excluir_ProdutoInexistente_LancaExcecao()
    {
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(
            () => _service.ExcluirAsync(999));

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Produto>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsultarEstoque_ProdutoInexistente_LancaExcecao()
    {
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(
            () => _service.ConsultarEstoqueAsync(999));
    }

    [Fact]
    public async Task Listar_ProdutosExistentes_RetornaLista()
    {
        var produtos = new List<Produto>
        {
            new Produto
            {
                Id = 1,
                Nome = "Notebook",
                Preco = 3000m,
                EstoqueAtual = 5,
                Ativo = true
            },
            new Produto
            {
                Id = 2,
                Nome = "Mouse",
                Preco = 100m,
                EstoqueAtual = 20,
                Ativo = true
            }
        };

        _repositoryMock
            .Setup(r => r.ListarAsync())
            .ReturnsAsync(produtos);

        var resultado = await _service.ListarAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Notebook", resultado[0].Nome);
        Assert.Equal("Mouse", resultado[1].Nome);

        _repositoryMock.Verify(
            r => r.ListarAsync(),
            Times.Once);
    }
}