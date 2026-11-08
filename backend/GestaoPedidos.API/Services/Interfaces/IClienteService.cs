using GestaoPedidos.API.DTOs.Clientes;

namespace GestaoPedidos.API.Services.Interfaces;

public interface IClienteService
{
    Task<List<ClienteResponse>> ListarAsync();
    Task<ClienteResponse> ObterPorIdAsync(int id);
    Task<ClienteResponse> AdicionarAsync(CriarClienteRequest request);
    Task<ClienteResponse> AtualizarAsync(int id, AtualizarClienteRequest request);
    Task ExcluirAsync(int id);
}