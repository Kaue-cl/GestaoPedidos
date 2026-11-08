using System.ComponentModel.DataAnnotations;

namespace GestaoPedidos.API.DTOs.Clientes;

public class CriarClienteRequest
{

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [RegularExpression(@"^\+?\d{10,11}$", ErrorMessage = "O telefone deve ter 10 ou 11 dígitos.")]
    public string Telefone { get; set; } = string.Empty;
}
     