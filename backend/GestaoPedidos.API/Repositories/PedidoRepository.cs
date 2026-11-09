using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.DTOs.Pedidos;

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

    
    private IQueryable<Pedido> AplicarFiltros(
        FiltroPedidosRequest filtro)
    {
        var query = _context.Pedidos.AsNoTracking().AsQueryable();

        if (filtro.Status.HasValue)
        {
            query = query.Where(p => p.Status == filtro.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Cliente))
        {
            var nomeCliente = filtro.Cliente.Trim();

            query = query.Where(p =>
                _context.Clientes.Any(c =>
                    c.Id == p.ClienteId &&
                    EF.Functions.Like(c.Nome, $"%{nomeCliente}%")));
        }

        return query;
    }

    public async Task<List<Pedido>> ListarAsync(
        FiltroPedidosRequest filtro)
    {
        IQueryable<Pedido> query = AplicarFiltros(filtro)
         .Include(p => p.Itens);

        var ascendente = filtro.Direcao == "asc";

        query = filtro.OrdenarPor switch
        {
            "valor" when ascendente =>
                query.OrderBy(p => p.ValorTotal)
                    .ThenBy(p => p.Id),

            "valor" =>
                query.OrderByDescending(p => p.ValorTotal)
                    .ThenByDescending(p => p.Id),

            "data" when ascendente =>
                query.OrderBy(p => p.DataPedido)
                    .ThenBy(p => p.Id),

            _ =>
                query.OrderByDescending(p => p.DataPedido)
                    .ThenByDescending(p => p.Id)
        };

        return await query
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync(
        FiltroPedidosRequest filtro)
    {
        return await AplicarFiltros(filtro).CountAsync();
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
            var linhasAfetadas = await _context.Pedidos
                .Where(p =>
                    p.Id == pedido.Id &&
                    p.Status == StatusPedido.Pendente)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        p => p.Status,
                        StatusPedido.Cancelado));

            if (linhasAfetadas == 0)
            {
                throw new StatusPedidoInvalidoException(
                    "O pedido não está pendente ou já foi cancelado.");
            }

            foreach (var item in pedido.Itens)
            {
                var produtosAtualizados = await _context.Produtos
                    .Where(p => p.Id == item.ProdutoId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            p => p.EstoqueAtual,
                            p => p.EstoqueAtual + item.Quantidade));

                if (produtosAtualizados != 1)
                {
                    throw new PedidoInvalidoException(
                        $"Não foi possível devolver o estoque do produto {item.ProdutoId}.");
                }
            }

            await transaction.CommitAsync();

            pedido.Status = StatusPedido.Cancelado;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}