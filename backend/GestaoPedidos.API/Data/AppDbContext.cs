using GestaoPedidos.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Configuração de relacionamento Cliente -> Pedido (1:N)
        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Pedidos)
            .WithOne(p => p.Cliente)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        //Configuração de relacionamento Pedido -> ItemPedido (1:N)
        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        //Configuração de relacionamento Produto -> ItemPedido (1:N)
        modelBuilder.Entity<Produto>()
            .HasMany(p => p.ItensPedido)
            .WithOne(i => i.Produto)
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        //Configuração de conversão para o status do pedido (enum -> string)
        modelBuilder.Entity<Pedido>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configuração de precisão para valores monetários
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ItemPedido>()
            .Property(i => i.PrecoUnitario)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ItemPedido>()
            .Property(i => i.Subtotal)
            .HasPrecision(18, 2);
    }
}