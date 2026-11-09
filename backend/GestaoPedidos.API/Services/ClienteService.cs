using GestaoPedidos.API.DTOs.Clientes;
using GestaoPedidos.API.Mappers;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services.Interfaces;
using GestaoPedidos.API.DTOs.Common;

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
        request.Email = request.Email.Trim();
        await ValidarEmailUnicoAsync(request.Email);

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

        request.Email = request.Email.Trim();

        await ValidarEmailUnicoAsync(request.Email, id);

        ClienteMapper.UpdateEntity(request, cliente);

        await _repository.AtualizarAsync(cliente);
        
        return ClienteMapper.toResponse(cliente);
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await _repository.ObterPorIdAsync(id)
            ?? throw new ClienteNaoEncontradoException(id);

        var possuiPedidos = await _repository.PossuiPedidosAsync(id);

        if (possuiPedidos)
        {
            throw new ClientePossuiPedidosException();
        }

        await _repository.ExcluirAsync(cliente);
    }

    private async Task ValidarEmailUnicoAsync(
        string email,
        int? ignorarClienteId = null)
    {
        var existe = await _repository.EmailExisteAsync(
            email,
            ignorarClienteId);

        if (existe)
        {
            throw new EmailJaCadastradoException(email);
        }
    }
    
    public async Task<PaginacaoResponse<ClienteResponse>> ListarPaginadoAsync(FiltroClientesRequest filtro)
    {
        var (clientes, totalRegistros) =
            await _repository.ListarPaginadoAsync(filtro);

        return new PaginacaoResponse<ClienteResponse>
        {
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)filtro.TamanhoPagina),
            Dados = clientes.Select(ClienteMapper.toResponse).ToList()
        };
    }
}
   