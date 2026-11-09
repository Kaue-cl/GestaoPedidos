using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.DTOs.Dashboard;

public class PedidosPorStatusResponse
{
    public StatusPedido Status { get; set; }
    public int Quantidade { get; set; }
}