using System.ComponentModel.DataAnnotations;

namespace GestaoPedidos.API.DTOs.Produtos;

public class AtualizarProdutoRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 2,
        ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "O estoque não pode ser negativo.")]
    public int EstoqueAtual { get; set; }

    [Required(ErrorMessage = "O status ativo é obrigatório.")]
    public bool? Ativo { get; set; }
}