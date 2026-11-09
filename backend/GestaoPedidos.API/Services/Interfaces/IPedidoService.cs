using GestaoPedidos.API.DTOs.Pedidos;

namespace GestaoPedidos.API.Services.Interfaces;

public interface IPedidoService
{
    Task<PedidoResponse> CriarAsync(
        CriarPedidoRequest request);

    Task<PedidoResponse> ObterPorIdAsync(int id);

    Task<List<PedidoResponse>> ListarAsync(
        int pagina,
        int tamanhoPagina);

    Task<int> ContarAsync();

    Task<PedidoResponse> AtualizarStatusAsync(
        int id,
        AtualizarStatusPedidoRequest request);

    Task<PedidoResponse> CancelarAsync(int id);
}