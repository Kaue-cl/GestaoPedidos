
using GestaoPedidos.API.DTOs.Dashboard;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services;
using Moq;
using Xunit;

namespace GestaoPedidos.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task ObterIndicadoresAsync_RetornaIndicadoresDoRepositorio()
    {
        // Arrange
        var indicadoresEsperados = new DashboardResponse
        {
            TotalPedidos = 5,
            FaturamentoTotal = 1000m,
            TicketMedio = 200m
        };

        var repositoryMock = new Mock<IDashboardRepository>();

        repositoryMock
            .Setup(r => r.ObterIndicadoresAsync())
            .ReturnsAsync(indicadoresEsperados);

        var service = new DashboardService(repositoryMock.Object);

        // Act
        var resultado = await service.ObterIndicadoresAsync();

        // Assert
        Assert.Same(indicadoresEsperados, resultado);

        repositoryMock.Verify(
            r => r.ObterIndicadoresAsync(),
            Times.Once);
    }
}
