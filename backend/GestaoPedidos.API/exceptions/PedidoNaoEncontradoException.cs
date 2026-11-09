namespace GestaoPedidos.API.Exceptions;

public class PedidoNaoEncontradoException : Exception
{
    public PedidoNaoEncontradoException(int id)
        : base($"Pedido com ID {id} não encontrado.")
    {
    }
}