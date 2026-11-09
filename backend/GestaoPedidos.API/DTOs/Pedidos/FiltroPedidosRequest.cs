
using GestaoPedidos.API.Enums;

namespace GestaoPedidos.API.DTOs.Pedidos;

public class FiltroPedidosRequest
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;

    public StatusPedido? Status { get; set; }
    public string? Cliente { get; set; }

    public string OrdenarPor { get; set; } = "data";
    public string Direcao { get; set; } = "desc";
}
