using GestaoPedidos.API.DTOs.Pedidos;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services;
using Moq;
using Xunit;

namespace GestaoPedidos.Tests.Services;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepository;
    private readonly Mock<IClienteRepository> _clienteRepository;
    private readonly Mock<IProdutoRepository> _produtoRepository;

    private readonly PedidoService _service;

    public PedidoServiceTests()
    {
        _pedidoRepository = new Mock<IPedidoRepository>();
        _clienteRepository = new Mock<IClienteRepository>();
        _produtoRepository = new Mock<IProdutoRepository>();

        _service = new PedidoService(
            _pedidoRepository.Object,
            _clienteRepository.Object,
            _produtoRepository.Object);
    }

    private void ConfigurarClienteValido()
    {
        _clienteRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(new Cliente
            {
                Id = 1,
                Nome = "Cliente Teste",
                Email = "teste@email.com"
            });
    }

    private void ConfigurarProdutoValido(
        int estoque = 10,
        decimal preco = 50m)
    {
        _produtoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(new Produto
            {
                Id = 1,
                Nome = "Produto Teste",
                Preco = preco,
                EstoqueAtual = estoque,
                Ativo = true
            });
    }

    private static CriarPedidoRequest CriarRequest(
        int quantidade = 2)
    {
        return new CriarPedidoRequest
        {
            ClienteId = 1,
            Itens = new List<CriarItemPedidoRequest>
            {
                new CriarItemPedidoRequest
                {
                    ProdutoId = 1,
                    Quantidade = quantidade
                }
            }
        };
    }

    [Fact]
    public async Task Criar_DadosValidos_CriaPedido()
    {
        ConfigurarClienteValido();
        ConfigurarProdutoValido();

        var resultado = await _service.CriarAsync(
            CriarRequest());

        Assert.Equal(StatusPedido.Pendente, resultado.Status);
        Assert.Single(resultado.Itens);

        _pedidoRepository.Verify(r =>
            r.CriarComAtualizacaoEstoqueAsync(
                It.IsAny<Pedido>(),
                It.IsAny<List<Produto>>()),
            Times.Once);
    }

    [Fact]
    public async Task Criar_ClienteInexistente_LancaExcecao()
    {
        _clienteRepository
            .Setup(r => r.ObterPorIdAsync(99))
            .ReturnsAsync((Cliente?)null);

        var request = CriarRequest();
        request.ClienteId = 99;

        await Assert.ThrowsAsync<PedidoInvalidoException>(
            () => _service.CriarAsync(request));
    }

    [Fact]
    public async Task Criar_ProdutoInexistente_LancaExcecao()
    {
        ConfigurarClienteValido();

        _produtoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync((Produto?)null);

        await Assert.ThrowsAsync<PedidoInvalidoException>(
            () => _service.CriarAsync(CriarRequest()));
    }

    [Fact]
    public async Task Criar_EstoqueInsuficiente_LancaExcecao()
    {
        ConfigurarClienteValido();
        ConfigurarProdutoValido(estoque: 1);

        await Assert.ThrowsAsync<EstoqueInsuficienteException>(
            () => _service.CriarAsync(
                CriarRequest(quantidade: 3)));

        _pedidoRepository.Verify(r =>
            r.CriarComAtualizacaoEstoqueAsync(
                It.IsAny<Pedido>(),
                It.IsAny<List<Produto>>()),
            Times.Never);
    }

    [Fact]
    public async Task Criar_DadosValidos_CalculaTotalCorretamente()
    {
        ConfigurarClienteValido();
        ConfigurarProdutoValido(preco: 25m);

        var resultado = await _service.CriarAsync(
            CriarRequest(quantidade: 3));

        Assert.Equal(75m, resultado.ValorTotal);
        Assert.Equal(75m, resultado.Itens[0].Subtotal);
        Assert.Equal(25m, resultado.Itens[0].PrecoUnitario);
    }

    [Fact]
    public async Task AtualizarStatus_TransicaoValida_AtualizaPedido()
    {
        var pedido = new Pedido
        {
            Id = 1,
            ClienteId = 1,
            Status = StatusPedido.Pendente
        };

        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(pedido);

        var request = new AtualizarStatusPedidoRequest
        {
            Status = StatusPedido.Pago
        };

        var resultado = await _service.AtualizarStatusAsync(
            1, request);

        Assert.Equal(StatusPedido.Pago, resultado.Status);

        _pedidoRepository.Verify(r =>
            r.AtualizarAsync(It.IsAny<Pedido>()),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarStatus_TransicaoInvalida_LancaExcecao()
    {
        var pedido = new Pedido
        {
            Id = 1,
            ClienteId = 1,
            Status = StatusPedido.Cancelado
        };

        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(pedido);

        var request = new AtualizarStatusPedidoRequest
        {
            Status = StatusPedido.Pago
        };

        await Assert.ThrowsAsync<StatusPedidoInvalidoException>(
            () => _service.AtualizarStatusAsync(1, request));

        _pedidoRepository.Verify(r =>
            r.AtualizarAsync(It.IsAny<Pedido>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancelar_PedidoPendente_DevolveEstoque()
    {
        var pedido = new Pedido
        {
            Id = 1,
            ClienteId = 1,
            Status = StatusPedido.Pendente,
            Itens = new List<ItemPedido>
            {
                new ItemPedido
                {
                    ProdutoId = 1,
                    Quantidade = 3,
                    PrecoUnitario = 20m,
                    Subtotal = 60m
                }
            }
        };

        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(pedido);

        _pedidoRepository
            .Setup(r => r.CancelarComDevolucaoEstoqueAsync(
                It.IsAny<Pedido>()))
            .Callback<Pedido>(p =>
                p.Status = StatusPedido.Cancelado)
            .Returns(Task.CompletedTask);

        var resultado = await _service.CancelarAsync(1);

        Assert.Equal(StatusPedido.Cancelado, resultado.Status);

        _pedidoRepository.Verify(r =>
            r.CancelarComDevolucaoEstoqueAsync(
                It.Is<Pedido>(p => p.Id == 1)),
            Times.Once);
    }
    
    [Fact]
    public async Task Criar_SemItens_LancaExcecao()
    {
        var request = new CriarPedidoRequest
        {
            ClienteId = 1,
            Itens = new List<CriarItemPedidoRequest>()
        };

        await Assert.ThrowsAsync<PedidoInvalidoException>(
            () => _service.CriarAsync(request));

        _pedidoRepository.Verify(
            r => r.CriarComAtualizacaoEstoqueAsync(
                It.IsAny<Pedido>(),
                It.IsAny<List<Produto>>()),
            Times.Never);
    }

    [Fact]
    public async Task Criar_ProdutoInativo_LancaExcecao()
    {
        ConfigurarClienteValido();

        _produtoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(new Produto
            {
                Id = 1,
                Nome = "Produto Inativo",
                Preco = 50m,
                EstoqueAtual = 10,
                Ativo = false
            });

        await Assert.ThrowsAsync<PedidoInvalidoException>(
            () => _service.CriarAsync(CriarRequest()));

        _pedidoRepository.Verify(
            r => r.CriarComAtualizacaoEstoqueAsync(
                It.IsAny<Pedido>(),
                It.IsAny<List<Produto>>()),
            Times.Never);
    }

    [Fact]
    public async Task Criar_ItensRepetidos_AgrupaQuantidades()
    {
        ConfigurarClienteValido();
        ConfigurarProdutoValido(estoque: 10, preco: 20m);

        var request = new CriarPedidoRequest
        {
            ClienteId = 1,
            Itens = new List<CriarItemPedidoRequest>
            {
                new CriarItemPedidoRequest
                {
                    ProdutoId = 1,
                    Quantidade = 2
                },
                new CriarItemPedidoRequest
                {
                    ProdutoId = 1,
                    Quantidade = 3
                }
            }
        };

        var resultado = await _service.CriarAsync(request);

        var item = Assert.Single(resultado.Itens);

        Assert.Equal(5, item.Quantidade);
        Assert.Equal(100m, resultado.ValorTotal);

        _pedidoRepository.Verify(
            r => r.CriarComAtualizacaoEstoqueAsync(
                It.Is<Pedido>(p =>
                    p.Itens.Count == 1 &&
                    p.Itens.First().Quantidade == 5),
                It.IsAny<List<Produto>>()),
            Times.Once);
    }

    [Fact]
    public async Task AtualizarStatus_PedidoInexistente_LancaExcecao()
    {
        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Pedido?)null);

        var request = new AtualizarStatusPedidoRequest
        {
            Status = StatusPedido.Pago
        };

        await Assert.ThrowsAsync<PedidoNaoEncontradoException>(
            () => _service.AtualizarStatusAsync(999, request));

        _pedidoRepository.Verify(
            r => r.AtualizarAsync(It.IsAny<Pedido>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancelar_PedidoPago_LancaExcecao()
    {
        var pedido = new Pedido
        {
            Id = 1,
            ClienteId = 1,
            Status = StatusPedido.Pago
        };

        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(pedido);

        await Assert.ThrowsAsync<StatusPedidoInvalidoException>(
            () => _service.CancelarAsync(1));

        _pedidoRepository.Verify(
            r => r.CancelarComDevolucaoEstoqueAsync(
                It.IsAny<Pedido>()),
            Times.Never);
    }

    [Fact]
    public async Task Cancelar_PedidoInexistente_LancaExcecao()
    {
        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<PedidoNaoEncontradoException>(
            () => _service.CancelarAsync(999));

        _pedidoRepository.Verify(
            r => r.CancelarComDevolucaoEstoqueAsync(
                It.IsAny<Pedido>()),
            Times.Never);
    }
}