namespace GestaoPedidos.API.DTOs.Dashboard;

public class DashboardResponse
{
    public int TotalPedidos { get; set; }
    public decimal FaturamentoTotal { get; set; }
    public decimal TicketMedio { get; set; }

    public List<PedidosPorStatusResponse> PedidosPorStatus { get; set; } = [];

    public List<ProdutoMaisVendidoResponse> ProdutosMaisVendidos { get; set; } = [];
}