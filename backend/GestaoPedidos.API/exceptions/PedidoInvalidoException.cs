namespace GestaoPedidos.API.Exceptions;

public class PedidoInvalidoException : Exception
{
    public PedidoInvalidoException(string mensagem)
        : base(mensagem)
    {
    }
}