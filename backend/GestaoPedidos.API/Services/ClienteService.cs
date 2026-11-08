using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.Mappers;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services.Interfaces;

namespace GestaoPedidos.API.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ClienteResponse>> ListarAsync()
    {
        var clientes = await _repository.ListarAsync();
        return clientes
            .Select(ClienteMapper.toResponse)
            .ToList();
    }

    public async Task<ClienteResponse> ObterPorIdAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id)
            ?? throw new ClienteNaoEncontradoException(id);

        return ClienteMapper.toResponse(cliente);
    }

    public async Task<ClienteResponse> AdicionarAsync(
        CriarClienteRequest request)
    {
        var cliente = ClienteMapper.toEntity(request);

        var clienteCriado = await _repository.AdicionarAsync(cliente);
        return ClienteMapper.toResponse(clienteCriado);
    }

    public async Task<ClienteResponse> AtualizarAsync(
        int id,
        AtualizarClienteRequest request)
    {
        var cliente = await _repository.ObterPorIdAsync(id)
            ?? throw new ClienteNaoEncontradoException(id);

        ClienteMapper.UpdateEntity(request, cliente);

        await _repository.AtualizarAsync(cliente);
        
        return ClienteMapper.toResponse(cliente);
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id)
            ?? throw new ClienteNaoEncontradoException(id);
    
        await _repository.ExcluirAsync(cliente);
    }

}
   