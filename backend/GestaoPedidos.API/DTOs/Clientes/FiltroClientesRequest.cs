
namespace GestaoPedidos.API.DTOs.Clientes;

public class FiltroClientesRequest
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string OrdenarPor { get; set; } = "nome";
    public string Direcao { get; set; } = "asc";
}
