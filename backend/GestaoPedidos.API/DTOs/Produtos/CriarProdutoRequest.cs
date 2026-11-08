namespace GestaoPedidos.API.DTOs.Produtos;

public class CriarProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int EstoqueAtual { get; set; }
    public bool Ativo { get; set; } = true;
}