
using GestaoPedidos.API.Data;
using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.DTOs.Pedidos;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Tests.Repositories;

public class FiltrosCaseInsensitiveTests
{
    private static async Task<(AppDbContext Context, SqliteConnection Connection)>
        CriarContextoAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return (context, connection);
    }

    [Fact]
    public async Task Clientes_FiltroNome_IgnoraMaiusculasMinusculas()
    {
        var (context, connection) = await CriarContextoAsync();

        await using (connection)
        await using (context)
        {
            context.Clientes.Add(new Cliente
            {
                Nome = "Carlos Almeida",
                Email = "carlos.filtro@exemplo.com"
            });

            await context.SaveChangesAsync();

            var repository = new ClienteRepository(context);

            var resultado = await repository.ListarPaginadoAsync(
                new FiltroClientesRequest
                {
                    Nome = "CARLOS",
                    Pagina = 1,
                    TamanhoPagina = 10
                });

            Assert.Contains(resultado.Dados,
                c => c.Nome == "Carlos Almeida");
        }
    }

    [Fact]
    public async Task Clientes_FiltroEmail_IgnoraMaiusculasMinusculas()
    {
        var (context, connection) = await CriarContextoAsync();

        await using (connection)
        await using (context)
        {
            context.Clientes.Add(new Cliente
            {
                Nome = "Cliente Email",
                Email = "contato.filtro@exemplo.com"
            });

            await context.SaveChangesAsync();

            var repository = new ClienteRepository(context);

            var resultado = await repository.ListarPaginadoAsync(
                new FiltroClientesRequest
                {
                    Email = "CONTATO.FILTRO",
                    Pagina = 1,
                    TamanhoPagina = 10
                });

            Assert.Contains(resultado.Dados,
                c => c.Email == "contato.filtro@exemplo.com");
        }
    }

    [Fact]
    public async Task Produtos_FiltroNome_IgnoraMaiusculasMinusculas()
    {
        var (context, connection) = await CriarContextoAsync();

        await using (connection)
        await using (context)
        {
            context.Produtos.Add(new Produto
            {
                Nome = "Notebook Especial",
                Preco = 3500m,
                EstoqueAtual = 10,
                Ativo = true
            });

            await context.SaveChangesAsync();

            var repository = new ProdutoRepository(context);

            var resultado = await repository.ListarPaginadoAsync(
                new FiltroProdutosRequest
                {
                    Nome = "NOTEBOOK",
                    Pagina = 1,
                    TamanhoPagina = 10
                });

            Assert.Contains(resultado.Dados,
                p => p.Nome == "Notebook Especial");
        }
    }

    [Fact]
    public async Task Pedidos_FiltroCliente_IgnoraMaiusculasMinusculas()
    {
        var (context, connection) = await CriarContextoAsync();

        await using (connection)
        await using (context)
        {
            var cliente = new Cliente
            {
                Nome = "Roberto Oliveira",
                Email = "roberto.filtro@exemplo.com"
            };

            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            context.Pedidos.Add(new Pedido
            {
                ClienteId = cliente.Id
            });

            await context.SaveChangesAsync();

            var repository = new PedidoRepository(context);

            var filtro = new FiltroPedidosRequest
            {
                Cliente = "ROBERTO",
                Pagina = 1,
                TamanhoPagina = 10
            };

            var resultado = await repository.ListarAsync(filtro);
            var total = await repository.ContarAsync(filtro);

            Assert.Contains(resultado,
                p => p.ClienteId == cliente.Id);

            Assert.Equal(1, total);
        }
    }

    
    [Fact]
    public async Task Produtos_FiltroParcial_MantemPaginacaoEContagem()
    {
        var (context, connection) = await CriarContextoAsync();

        await using (connection)
        await using (context)
        {
            context.Produtos.AddRange(
                new Produto
                {
                    Nome = "ProdutoFiltroXYZ Modelo A",
                    Preco = 250m,
                    EstoqueAtual = 5,
                    Ativo = true
                },
                new Produto
                {
                    Nome = "ProdutoFiltroXYZ Modelo B",
                    Preco = 180m,
                    EstoqueAtual = 8,
                    Ativo = true
                });

            await context.SaveChangesAsync();

            var repository = new ProdutoRepository(context);

            var resultado = await repository.ListarPaginadoAsync(
                new FiltroProdutosRequest
                {
                    Nome = "PRODUTOFILTROXYZ",
                    Pagina = 1,
                    TamanhoPagina = 1
                });

            Assert.Single(resultado.Dados);
            Assert.Equal(2, resultado.TotalRegistros);
        }
    }
}
