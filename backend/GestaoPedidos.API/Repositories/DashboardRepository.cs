using GestaoPedidos.API.Data;
using GestaoPedidos.API.DTOs.Dashboard;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.API.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> ObterIndicadoresAsync()
    {
        var statusFaturados = new[]
        {
            StatusPedido.Pago,
            StatusPedido.Enviado,
            StatusPedido.Entregue
        };

        // Total de pedidos, incluindo cancelados
        var totalPedidos = await _context.Pedidos
            .CountAsync();

        // Pedidos considerados no faturamento
        var pedidosFaturados = _context.Pedidos
            .AsNoTracking()
            .Where(p => statusFaturados.Contains(p.Status));

        var quantidadeFaturados = await pedidosFaturados
            .CountAsync();

        var faturamentoTotal = await pedidosFaturados
            .SumAsync(p => (decimal?)p.ValorTotal) ?? 0m;

        var ticketMedio = quantidadeFaturados > 0
            ? faturamentoTotal / quantidadeFaturados
            : 0m;

        // Distribuição dos pedidos por status
        var pedidosPorStatus = await _context.Pedidos
            .AsNoTracking()
            .GroupBy(p => p.Status)
            .Select(g => new PedidosPorStatusResponse
            {
                Status = g.Key,
                Quantidade = g.Count()
            })
            .OrderBy(p => p.Status)
            .ToListAsync();

        // Produtos mais vendidos, desconsiderando pedidos cancelados
        var produtosMaisVendidos = await _context.ItensPedido
            .AsNoTracking()
            .Where(i => i.Pedido.Status != StatusPedido.Cancelado)
            .GroupBy(i => new
            {
                i.ProdutoId,
                i.Produto.Nome
            })
            .Select(g => new ProdutoMaisVendidoResponse
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Nome,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                ValorTotalVendido = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .ThenBy(p => p.Nome)
            .Take(5)
            .ToListAsync();

        return new DashboardResponse
        {
            TotalPedidos = totalPedidos,
            FaturamentoTotal = faturamentoTotal,
            TicketMedio = Math.Round(ticketMedio, 2),
            PedidosPorStatus = pedidosPorStatus,
            ProdutosMaisVendidos = produtosMaisVendidos
        };
    }
}