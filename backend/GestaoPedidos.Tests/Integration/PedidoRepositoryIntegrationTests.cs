using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestaoPedidos.Tests.Integration;

public class PedidoRepositoryIntegrationTests
{
    [Fact]
    public async Task CriarPedido_DevePersistirPedidoEDescontarEstoque()
    {
        var caminhoBanco = Path.Combine(
            Path.GetTempPath(),
            $"gestaopedidos-test-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={caminhoBanco}")
                .Options;

            await using (var context = new AppDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var cliente = new Cliente
                {
                    Nome = "Cliente Integração",
                    Email = "integracao@teste.com",
                    DataCriacao = DateTime.UtcNow
                };

                var produto = new Produto
                {
                    Nome = "Produto Integração",
                    Preco = 25m,
                    EstoqueAtual = 5,
                    Ativo = true
                };

                context.Clientes.Add(cliente);
                context.Produtos.Add(produto);

                await context.SaveChangesAsync();

                var pedido = new Pedido
                {
                    ClienteId = cliente.Id,
                    DataPedido = DateTime.UtcNow,
                    Status = StatusPedido.Pendente,
                    ValorTotal = 75m,
                    Itens = new List<ItemPedido>
                    {
                        new ItemPedido
                        {
                            ProdutoId = produto.Id,
                            Quantidade = 3,
                            PrecoUnitario = 25m,
                            Subtotal = 75m
                        }
                    }
                };

                var repository = new PedidoRepository(context);

                await repository.CriarComAtualizacaoEstoqueAsync(
                    pedido,
                    new List<Produto> { produto });
            }

            await using (var context = new AppDbContext(options))
            {
                var pedidoSalvo = await context.Pedidos
                    .Include(p => p.Itens)
                    .SingleAsync(p => p.Cliente.Email == "integracao@teste.com");

                var produtoAtualizado = await context.Produtos
                    .SingleAsync(p => p.Nome == "Produto Integração");

                Assert.Equal(75m, pedidoSalvo.ValorTotal);
                Assert.Equal(StatusPedido.Pendente, pedidoSalvo.Status);
                Assert.Single(pedidoSalvo.Itens);
                Assert.Equal(3, pedidoSalvo.Itens.First().Quantidade);
                Assert.Equal(2, produtoAtualizado.EstoqueAtual);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(caminhoBanco))
                File.Delete(caminhoBanco);
        }
    }
}