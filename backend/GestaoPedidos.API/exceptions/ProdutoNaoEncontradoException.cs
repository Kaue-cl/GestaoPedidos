namespace GestaoPedidos.API.Exceptions;

public class ProdutoNaoEncontradoException : Exception
{
    public ProdutoNaoEncontradoException(int id)
        : base($"Produto com ID {id} não encontrado.")
    {
    }
}