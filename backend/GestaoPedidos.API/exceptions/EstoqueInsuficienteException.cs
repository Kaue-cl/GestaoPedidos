namespace GestaoPedidos.API.Exceptions;

public class EstoqueInsuficienteException : Exception
{
    public EstoqueInsuficienteException(
        string nomeProduto,
        int estoqueDisponivel)
        : base(
            $"Estoque insuficiente para o produto " +
            $"'{nomeProduto}'. Disponível: {estoqueDisponivel}.")
    {
    }
}