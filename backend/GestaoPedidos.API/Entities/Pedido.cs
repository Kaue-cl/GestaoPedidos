using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public StatusPedido Status { get; set; } = StatusPedido.Pendente;
    public decimal ValorTotal { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}