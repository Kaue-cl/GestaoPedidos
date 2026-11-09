using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pedido?> ObterPorIdAsync(int id)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Pedido>> ListarAsync(
        int pagina,
        int tamanhoPagina)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync()
    {
        return await _context.Pedidos.CountAsync();
    }

    public async Task AdicionarAsync(Pedido pedido)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Pedido pedido)
    {
        _context.Pedidos.Update(pedido);
        await _context.SaveChangesAsync();
    }

    public async Task CriarComAtualizacaoEstoqueAsync(
    Pedido pedido,
    List<Produto> produtos)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in pedido.Itens)
            {
                var linhasAfetadas = await _context.Produtos
                    .Where(p =>
                        p.Id == item.ProdutoId &&
                        p.Ativo &&
                        p.EstoqueAtual >= item.Quantidade)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            p => p.EstoqueAtual,
                            p => p.EstoqueAtual - item.Quantidade));

                if (linhasAfetadas == 0)
                {
                    throw new PedidoInvalidoException(
                        $"Estoque indisponível ou produto inválido: " +
                        $"{item.ProdutoId}.");
                }
            }

            await _context.Pedidos.AddAsync(pedido);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CancelarComDevolucaoEstoqueAsync(
    Pedido pedido)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in pedido.Itens)
            {
                await _context.Produtos
                    .Where(p => p.Id == item.ProdutoId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            p => p.EstoqueAtual,
                            p => p.EstoqueAtual + item.Quantidade));
            }

            pedido.Status = StatusPedido.Cancelado;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}