using GestaoPedidos.API.DTOs.Pedidos;
using GestaoPedidos.API.Entities;

namespace GestaoPedidos.API.Mappers;

public static class PedidoMapper
{
    public static PedidoResponse ParaResponse(Pedido pedido)
    {
        return new PedidoResponse
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            DataPedido = pedido.DataPedido,
            Status = pedido.Status,
            ValorTotal = pedido.ValorTotal,

            Itens = pedido.Itens
                .Select(ParaItemResponse)
                .ToList()
        };
    }

    public static ItemPedidoResponse ParaItemResponse(ItemPedido item)
    {
        return new ItemPedidoResponse
        {
            Id = item.Id,
            ProdutoId = item.ProdutoId,
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario,
            Subtotal = item.Subtotal
        };
    }
}