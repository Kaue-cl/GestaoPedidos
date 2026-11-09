using GestaoPedidos.API.Services;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.DTOs.Clientes;
using Moq;

namespace GestaoPedidos.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _repositoryMock;
    private readonly ClienteService _service;

    public ClienteServiceTests()
    {
        _repositoryMock = new Mock<IClienteRepository>();
        _service = new ClienteService(_repositoryMock.Object);
    }

    [Fact]
    public async Task OberPorId_ClienteExistente_RetornaCliente()
    {
        var clienteId = new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Email = "joao@exemplo.com",
            Telefone = "11999990001"
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(clienteId.Id))
            .ReturnsAsync(clienteId);
        
        var resultado = await _service.ObterPorIdAsync(clienteId.Id);
        
        Assert.Equal(clienteId.Id, resultado.Id);
        Assert.Equal(clienteId.Nome, resultado.Nome);
        Assert.Equal(clienteId.Email, resultado.Email);
        Assert.Equal(clienteId.Telefone, resultado.Telefone);
        Assert.Equal("João Silva", resultado.Nome);
    }

    [Fact]
    public async Task ObterPorId_ClienteInexistente_LancaExcecao()
    {
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ClienteNaoEncontradoException>(
            () => _service.ObterPorIdAsync(999));
    }

    [Fact]
    public async Task Adicionar_DadosValidos_CadastraCliente()
    {
        var request = new CriarClienteRequest
        {
            Nome = "Carlos Almeida",
            Email = "carlos@exemplo.com",
            Telefone = "11988887777"
        };

        _repositoryMock
            .Setup(r => r.EmailExisteAsync(
                request.Email,
                It.IsAny<int?>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Cliente>()))
            .ReturnsAsync(new Cliente
            {
                Id = 4,
                Nome = request.Nome,
                Email = request.Email,
                Telefone = request.Telefone
            });

        var resultado = await _service.AdicionarAsync(request);

        Assert.Equal(4, resultado.Id);
        Assert.Equal("Carlos Almeida", resultado.Nome);

        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Cliente>()),
            Times.Once);
    }

    [Fact]
    public async Task Adicionar_EmailDuplicado_LancaExcecao()
    {
        var request = new CriarClienteRequest
        {
            Nome = "Carlos Almeida",
            Email = "joao@exemplo.com",
            Telefone = "11988887777"
        };

        _repositoryMock
            .Setup(r => r.EmailExisteAsync(
                request.Email,
                It.IsAny<int?>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<EmailJaCadastradoException>(
            () => _service.AdicionarAsync(request));

        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<Cliente>()),
            Times.Never);
    }

    [Fact]
    public async Task Excluir_ClienteInexistente_LancaExcecao()
    {
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ClienteNaoEncontradoException>(
            () => _service.ExcluirAsync(999));

        _repositoryMock.Verify(
            r => r.ExcluirAsync(It.IsAny<Cliente>()),
            Times.Never);
    }
        
    [Fact]
    public async Task Listar_ClientesExistentes_RetornaLista()
    {
        // Arrange
        var clientes = new List<Cliente>
        {
            new Cliente
            {
                Id = 1,
                Nome = "João Silva",
                Email = "joao@exemplo.com"
            },
            new Cliente
            {
                Id = 2,
                Nome = "Maria Souza",
                Email = "maria@exemplo.com"
            }
        };

        _repositoryMock
            .Setup(r => r.ListarAsync())
            .ReturnsAsync(clientes);

        // Act
        var resultado = await _service.ListarAsync();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.Equal("João Silva", resultado[0].Nome);
        Assert.Equal("Maria Souza", resultado[1].Nome);

        _repositoryMock.Verify(
            r => r.ListarAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Atualizar_ClienteExistente_AtualizaDados()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Email = "joao@exemplo.com",
            Telefone = "11999990001"
        };

        var request = new AtualizarClienteRequest
        {
            Nome = "João Atualizado",
            Email = "joao.novo@exemplo.com",
            Telefone = "11988887777"
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(cliente);

        _repositoryMock
            .Setup(r => r.EmailExisteAsync(request.Email, 1))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.AtualizarAsync(1, request);

        // Assert
        Assert.Equal("João Atualizado", resultado.Nome);
        Assert.Equal("joao.novo@exemplo.com", resultado.Email);
        Assert.Equal("11988887777", resultado.Telefone);

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Cliente>()),
            Times.Once);
    }

    [Fact]
    public async Task Excluir_ClienteSemPedidos_ExcluiCliente()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 1,
            Nome = "João Silva",
            Email = "joao@exemplo.com"
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(1))
            .ReturnsAsync(cliente);

        _repositoryMock
            .Setup(r => r.PossuiPedidosAsync(1))
            .ReturnsAsync(false);

        // Act
        await _service.ExcluirAsync(1);

        // Assert
        _repositoryMock.Verify(
            r => r.PossuiPedidosAsync(1),
            Times.Once);

        _repositoryMock.Verify(
            r => r.ExcluirAsync(cliente),
            Times.Once);
    }

    [Fact]
    public async Task Excluir_ClienteComPedidos_LancaExcecao()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 2,
            Nome = "Maria Souza",
            Email = "maria@exemplo.com"
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(2))
            .ReturnsAsync(cliente);

        _repositoryMock
            .Setup(r => r.PossuiPedidosAsync(2))
            .ReturnsAsync(true);

        // Act e Assert
        var excecao = await Assert.ThrowsAsync<ClientePossuiPedidosException>(
            () => _service.ExcluirAsync(2));

        Assert.Equal(
            "Não é possível excluir o cliente, pois existem pedidos associados a ele.",
            excecao.Message);

        _repositoryMock.Verify(
            r => r.PossuiPedidosAsync(2),
            Times.Once);

        _repositoryMock.Verify(
            r => r.ExcluirAsync(It.IsAny<Cliente>()),
            Times.Never);
    }

    [Fact]
    public async Task Atualizar_ClienteInexistente_LancaExcecao()
    {
        // Arrange
        var request = new AtualizarClienteRequest
        {
            Nome = "Cliente Atualizado",
            Email = "cliente@exemplo.com",
            Telefone = "11999990000"
        };

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(999))
            .ReturnsAsync((Cliente?)null);

        // Act e Assert
        await Assert.ThrowsAsync<ClienteNaoEncontradoException>(
            () => _service.AtualizarAsync(999, request));

        _repositoryMock.Verify(
            r => r.AtualizarAsync(It.IsAny<Cliente>()),
            Times.Never);
    }

}