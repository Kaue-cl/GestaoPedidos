namespace GestaoPedidos.API.DTOs.Produtos;

public class ProdutoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int EstoqueAtual { get; set; }
    public bool Ativo { get; set; }
}