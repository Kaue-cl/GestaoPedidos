using GestaoPedidos.API.Entities;
using GestaoPedidos.API.DTOs.Clientes;

namespace GestaoPedidos.API.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<List<Cliente>> ListarAsync();
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente> AdicionarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task ExcluirAsync(Cliente cliente);
    Task<bool> EmailExisteAsync(
        string email,
        int? ignorarClienteId = null);
    Task<(List<Cliente> Dados, int TotalRegistros)> ListarPaginadoAsync(
        FiltroClientesRequest filtro);
}