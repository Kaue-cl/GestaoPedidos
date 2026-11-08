using GestaoPedidos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.API.Data;

public static class DbInitializer
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente
            {
                Id = 1,
                Nome = "João Silva",
                Email = "joao@exemplo.com",
                Telefone = "11999990001",
                DataCriacao = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Cliente
            {
                Id = 2,
                Nome = "Maria Oliveira",
                Email = "maria@exemplo.com",
                Telefone = "11999990002",
                DataCriacao = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Cliente
            {
                Id = 3,
                Nome = "Pedro Santos",
                Email = "pedro@exemplo.com",
                Telefone = "11999990003",
                DataCriacao = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Produto>().HasData(
            new Produto
            {
                Id = 1,
                Nome = "Notebook Dell",
                Preco = 3500.00m,
                EstoqueAtual = 10,
                Ativo = true
            },
            new Produto
            {
                Id = 2,
                Nome = "Mouse Logitech",
                Preco = 120.00m,
                EstoqueAtual = 25,
                Ativo = true
            },
            new Produto
            {
                Id = 3,
                Nome = "Teclado Mecânico",
                Preco = 250.00m,
                EstoqueAtual = 15,
                Ativo = true
            },
            new Produto
            {
                Id = 4,
                Nome = "Monitor 24 polegadas",
                Preco = 900.00m,
                EstoqueAtual = 8,
                Ativo = true
            },
            new Produto
            {
                Id = 5,
                Nome = "Headset Gamer",
                Preco = 180.00m,
                EstoqueAtual = 20,
                Ativo = true
            }
        );
    }
}