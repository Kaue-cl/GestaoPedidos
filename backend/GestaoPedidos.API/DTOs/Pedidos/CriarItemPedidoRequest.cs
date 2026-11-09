using System.ComponentModel.DataAnnotations;

namespace GestaoPedidos.API.DTOs.Pedidos;

public class CriarItemPedidoRequest
{
    [Range(1, int.MaxValue,
        ErrorMessage = "O produto deve ser válido.")]
    public int ProdutoId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}