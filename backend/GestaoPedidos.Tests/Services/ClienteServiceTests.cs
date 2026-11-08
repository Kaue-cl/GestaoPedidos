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
}