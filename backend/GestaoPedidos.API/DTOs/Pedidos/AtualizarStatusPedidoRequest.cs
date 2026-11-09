using System.ComponentModel.DataAnnotations;
using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.DTOs.Pedidos;

public class AtualizarStatusPedidoRequest
{
    [Required]
    [EnumDataType(typeof(StatusPedido),
        ErrorMessage = "Status do pedido inválido.")]
    public StatusPedido? Status { get; set; }
}