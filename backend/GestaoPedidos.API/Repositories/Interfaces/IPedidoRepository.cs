using GestaoPedidos.API.Entities;
using GestaoPedidos.API.DTOs.Pedidos;

namespace GestaoPedidos.API.Repositories.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(int id);

    Task<List<Pedido>> ListarAsync(FiltroPedidosRequest filtro);

Task<int> ContarAsync(FiltroPedidosRequest filtro);

    Task AdicionarAsync(Pedido pedido);

    Task AtualizarAsync(Pedido pedido);

    Task CriarComAtualizacaoEstoqueAsync(
        Pedido pedido,
        List<Produto> produtos);

    Task CancelarComDevolucaoEstoqueAsync(Pedido pedido);
}