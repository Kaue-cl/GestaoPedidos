using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.DTOs.Pedidos;

public class PedidoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataPedido { get; set; }
    public StatusPedido Status { get; set; }
    public decimal ValorTotal { get; set; }

    public List<ItemPedidoResponse> Itens { get; set; } = new();
}