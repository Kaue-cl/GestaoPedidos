using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestaoPedidos.Tests.Integration;

public class DashboardRepositoryIntegrationTests
{
    private static DbContextOptions<AppDbContext> CriarOptions(
        string caminhoBanco)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={caminhoBanco}")
            .Options;
    }

    private static async Task ExecutarTesteAsync(
        Func<AppDbContext, Task> teste)
    {
        var caminhoBanco = Path.Combine(
            Path.GetTempPath(),
            $"dashboard-test-{Guid.NewGuid():N}.db");

        try
        {
            var options = CriarOptions(caminhoBanco);

            await using var context = new AppDbContext(options);

            await context.Database.EnsureCreatedAsync();

            await teste(context);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(caminhoBanco))
                File.Delete(caminhoBanco);
        }
    }

    private static async Task<(Cliente cliente, Produto produto)>
        CriarDadosBaseAsync(AppDbContext context)
    {
        var cliente = new Cliente
        {
            Nome = "Cliente Dashboard",
            Email = "dashboard@teste.com",
            DataCriacao = DateTime.UtcNow
        };

        var produto = new Produto
        {
            Nome = "Produto Dashboard",
            Preco = 50m,
            EstoqueAtual = 100,
            Ativo = true
        };

        context.Clientes.Add(cliente);
        context.Produtos.Add(produto);

        await context.SaveChangesAsync();

        return (cliente, produto);
    }

    private static async Task CriarPedidoAsync(
        AppDbContext context,
        int clienteId,
        int produtoId,
        StatusPedido status,
        int quantidade,
        decimal precoUnitario = 50m)
    {
        var pedido = new Pedido
        {
            ClienteId = clienteId,
            DataPedido = DateTime.UtcNow,
            Status = status,
            ValorTotal = quantidade * precoUnitario,
            Itens = new List<ItemPedido>
            {
                new ItemPedido
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade,
                    PrecoUnitario = precoUnitario,
                    Subtotal = quantidade * precoUnitario
                }
            }
        };

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Dashboard_SemPedidos_RetornaIndicadoresZerados()
    {
        await ExecutarTesteAsync(async context =>
        {
            var repository = new DashboardRepository(context);

            var resultado = await repository.ObterIndicadoresAsync();

            Assert.Equal(0, resultado.TotalPedidos);
            Assert.Equal(0m, resultado.FaturamentoTotal);
            Assert.Equal(0m, resultado.TicketMedio);
            Assert.Empty(resultado.PedidosPorStatus);
            Assert.Empty(resultado.ProdutosMaisVendidos);
        });
    }

    [Fact]
    public async Task Dashboard_CalculaFaturamentoETicketMedio()
    {
        await ExecutarTesteAsync(async context =>
        {
            var (cliente, produto) =
                await CriarDadosBaseAsync(context);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pago, 2);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Entregue, 4);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pendente, 3);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Cancelado, 5);

            var repository = new DashboardRepository(context);

            var resultado = await repository.ObterIndicadoresAsync();

            Assert.Equal(4, resultado.TotalPedidos);
            Assert.Equal(300m, resultado.FaturamentoTotal);
            Assert.Equal(150m, resultado.TicketMedio);
        });
    }

    [Fact]
    public async Task Dashboard_AgrupaPedidosPorStatus()
    {
        await ExecutarTesteAsync(async context =>
        {
            var (cliente, produto) =
                await CriarDadosBaseAsync(context);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pendente, 1);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pendente, 1);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pago, 1);

            var repository = new DashboardRepository(context);

            var resultado = await repository.ObterIndicadoresAsync();

            Assert.Equal(3, resultado.TotalPedidos);

            Assert.Contains(resultado.PedidosPorStatus,
                x => x.Status == StatusPedido.Pendente
                     && x.Quantidade == 2);

            Assert.Contains(resultado.PedidosPorStatus,
                x => x.Status == StatusPedido.Pago
                     && x.Quantidade == 1);
        });
    }

    [Fact]
    public async Task Dashboard_ProdutosMaisVendidos_ExcluiCancelados()
    {
        await ExecutarTesteAsync(async context =>
        {
            var (cliente, produto) =
                await CriarDadosBaseAsync(context);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pago, 3);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Pendente, 2);

            await CriarPedidoAsync(
                context, cliente.Id, produto.Id,
                StatusPedido.Cancelado, 10);

            var repository = new DashboardRepository(context);

            var resultado = await repository.ObterIndicadoresAsync();

            var produtoRanking = Assert.Single(
                resultado.ProdutosMaisVendidos);

            Assert.Equal(produto.Id, produtoRanking.ProdutoId);
            Assert.Equal(5, produtoRanking.QuantidadeVendida);
            Assert.Equal(250m, produtoRanking.ValorTotalVendido);
        });
    }

    [Fact]
    public async Task Dashboard_Ranking_RetornaSomenteCincoProdutos()
    {
        await ExecutarTesteAsync(async context =>
        {
            var cliente = new Cliente
            {
                Nome = "Cliente Ranking",
                Email = "ranking@teste.com",
                DataCriacao = DateTime.UtcNow
            };

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            for (int i = 1; i <= 6; i++)
            {
                var produto = new Produto
                {
                    Nome = $"Produto Ranking {i}",
                    Preco = 10m,
                    EstoqueAtual = 100,
                    Ativo = true
                };

                context.Produtos.Add(produto);
                await context.SaveChangesAsync();

                await CriarPedidoAsync(
                    context,
                    cliente.Id,
                    produto.Id,
                    StatusPedido.Pago,
                    i,
                    10m);
            }

            var repository = new DashboardRepository(context);

            var resultado = await repository.ObterIndicadoresAsync();

            Assert.Equal(5, resultado.ProdutosMaisVendidos.Count);

            Assert.Equal(
                6,
                resultado.ProdutosMaisVendidos[0].QuantidadeVendida);

            Assert.DoesNotContain(
                resultado.ProdutosMaisVendidos,
                p => p.Nome == "Produto Ranking 1");
        });
    }
}