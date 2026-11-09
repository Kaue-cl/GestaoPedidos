namespace GestaoPedidos.API.DTOs.Dashboard;

public class ProdutoMaisVendidoResponse
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal ValorTotalVendido { get; set; }
}