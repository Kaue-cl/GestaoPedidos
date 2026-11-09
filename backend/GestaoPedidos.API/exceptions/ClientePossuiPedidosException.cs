namespace GestaoPedidos.API.Exceptions;

public class ClientePossuiPedidosException : Exception
{
    public ClientePossuiPedidosException()
        : base("Não é possível excluir o cliente, pois existem pedidos associados a ele.")
    {
    }
}