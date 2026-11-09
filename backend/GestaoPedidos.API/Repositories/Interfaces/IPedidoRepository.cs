using GestaoPedidos.API.Entities;

namespace GestaoPedidos.API.Repositories.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(int id);

    Task<List<Pedido>> ListarAsync(
        int pagina,
        int tamanhoPagina);

    Task<int> ContarAsync();

    Task AdicionarAsync(Pedido pedido);

    Task AtualizarAsync(Pedido pedido);

    Task CriarComAtualizacaoEstoqueAsync(
        Pedido pedido,
        List<Produto> produtos);

    Task CancelarComDevolucaoEstoqueAsync(Pedido pedido);
}