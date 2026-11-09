using System.ComponentModel.DataAnnotations;

namespace GestaoPedidos.API.DTOs.Pedidos;

public class CriarPedidoRequest
{
    [Range(1, int.MaxValue,
        ErrorMessage = "O cliente deve ser válido.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Informe os itens do pedido.")]
    [MinLength(1,
        ErrorMessage = "O pedido deve conter pelo menos um item.")]
    public List<CriarItemPedidoRequest> Itens { get; set; } = new();
}