using GestaoPedidos.API.DTOs.Pedidos;

namespace GestaoPedidos.API.Services.Interfaces;

public interface IPedidoService
{
    Task<PedidoResponse> CriarAsync(
        CriarPedidoRequest request);

    Task<PedidoResponse> ObterPorIdAsync(int id);

    Task<List<PedidoResponse>> ListarAsync(FiltroPedidosRequest filtro);

    Task<int> ContarAsync(FiltroPedidosRequest filtro);

    Task<PedidoResponse> AtualizarStatusAsync(
        int id,
        AtualizarStatusPedidoRequest request);

    Task<PedidoResponse> CancelarAsync(int id);
}