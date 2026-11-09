namespace GestaoPedidos.API.Exceptions;

public class StatusPedidoInvalidoException : Exception
{
    public StatusPedidoInvalidoException(string mensagem)
        : base(mensagem)
    {
    }
}