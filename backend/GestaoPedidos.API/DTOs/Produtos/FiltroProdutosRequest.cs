
namespace GestaoPedidos.API.DTOs.Produtos;

public class FiltroProdutosRequest
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? Nome { get; set; }
    public bool? Ativo { get; set; }
    public string OrdenarPor { get; set; } = "nome";
    public string Direcao { get; set; } = "asc";
}
