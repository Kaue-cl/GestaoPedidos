using GestaoPedidos.API.DTOs.Pedidos;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services.Interfaces;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Enums;
using GestaoPedidos.API.Exceptions;
using GestaoPedidos.API.Mappers;

namespace GestaoPedidos.API.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IClienteRepository clienteRepository,
        IProdutoRepository produtoRepository)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<PedidoResponse> CriarAsync(
    CriarPedidoRequest request)
    {
        if (request is null)
        {
            throw new PedidoInvalidoException(
                "Os dados do pedido são obrigatórios.");
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new PedidoInvalidoException(
                "O pedido deve possuir pelo menos um item.");
        }

        var cliente = await _clienteRepository
            .ObterPorIdAsync(request.ClienteId);

        if (cliente is null)
        {
            throw new PedidoInvalidoException(
                $"Cliente com ID {request.ClienteId} não encontrado.");
        }

        var itensAgrupados = request.Itens
            .GroupBy(i => i.ProdutoId)
            .Select(g => new
            {
                ProdutoId = g.Key,
                Quantidade = g.Sum(i => (long)i.Quantidade)
            })
            .ToList();

        var itensPedido = new List<ItemPedido>();
        var produtos = new List<Produto>();

        foreach (var item in itensAgrupados)
        {
            if (item.ProdutoId <= 0 ||
                item.Quantidade <= 0 ||
                item.Quantidade > int.MaxValue)
            {
                throw new PedidoInvalidoException(
                    "Os itens do pedido possuem dados inválidos.");
            }

            var produto = await _produtoRepository
                .ObterPorIdAsync(item.ProdutoId);

            if (produto is null)
            {
                throw new PedidoInvalidoException(
                    $"Produto com ID {item.ProdutoId} não encontrado.");
            }

            if (!produto.Ativo)
            {
                throw new PedidoInvalidoException(
                    $"O produto '{produto.Nome}' está inativo.");
            }

            var quantidade = (int)item.Quantidade;

            if (produto.EstoqueAtual < quantidade)
            {
                throw new EstoqueInsuficienteException(
                    produto.Nome,
                    produto.EstoqueAtual);
            }

            var subtotal = produto.Preco * quantidade;

            itensPedido.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco,
                Subtotal = subtotal
            });

            produtos.Add(produto);
        }

        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedido.Pendente,
            Itens = itensPedido,
            ValorTotal = itensPedido.Sum(i => i.Subtotal)
        };

        await _pedidoRepository
            .CriarComAtualizacaoEstoqueAsync(pedido, produtos);

        return PedidoMapper.ParaResponse(pedido);
    }

    public async Task<PedidoResponse> ObterPorIdAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);

        if (pedido is null)
            throw new PedidoNaoEncontradoException(id);

        return PedidoMapper.ParaResponse(pedido);
    }

    
    public async Task<List<PedidoResponse>> ListarAsync(
        FiltroPedidosRequest filtro)
    {
        if (filtro.Pagina < 1 ||
            filtro.TamanhoPagina < 1 ||
            filtro.TamanhoPagina > 100)
        {
            throw new PedidoInvalidoException(
                "A página deve ser maior que zero e o tamanho " +
                "deve estar entre 1 e 100.");
        }

        filtro.OrdenarPor = filtro.OrdenarPor?.Trim().ToLowerInvariant()
            ?? "data";

        filtro.Direcao = filtro.Direcao?.Trim().ToLowerInvariant()
            ?? "desc";

        if (filtro.OrdenarPor != "data" &&
            filtro.OrdenarPor != "valor")
        {
            throw new PedidoInvalidoException(
                "A ordenação deve ser por 'data' ou 'valor'.");
        }

        if (filtro.Direcao != "asc" &&
            filtro.Direcao != "desc")
        {
            throw new PedidoInvalidoException(
                "A direção deve ser 'asc' ou 'desc'.");
        }

        if (filtro.Status.HasValue &&
            !Enum.IsDefined(typeof(StatusPedido), filtro.Status.Value))
        {
            throw new PedidoInvalidoException(
                "Informe um status válido.");
        }

        filtro.Cliente = filtro.Cliente?.Trim();

        var pedidos = await _pedidoRepository.ListarAsync(filtro);

        return pedidos
            .Select(PedidoMapper.ParaResponse)
            .ToList();
    }

    public async Task<int> ContarAsync(FiltroPedidosRequest filtro)
    {
        return await _pedidoRepository.ContarAsync(filtro);
    }


    public async Task<PedidoResponse> AtualizarStatusAsync(
    int id,
    AtualizarStatusPedidoRequest request)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);

        if (pedido is null)
            throw new PedidoNaoEncontradoException(id);

        if (request is null || request.Status is null)
        {
            throw new StatusPedidoInvalidoException(
                "Informe um status válido.");
        }

        var novoStatus = request.Status.Value;

        bool transicaoValida = pedido.Status switch
        {
            StatusPedido.Pendente =>
                novoStatus == StatusPedido.Pago,

            StatusPedido.Pago =>
                novoStatus == StatusPedido.Enviado,

            StatusPedido.Enviado =>
                novoStatus == StatusPedido.Entregue,

            _ => false
        };

        if (!transicaoValida)
        {
            throw new StatusPedidoInvalidoException(
                $"Não é permitido alterar o status " +
                $"de {pedido.Status} para {novoStatus}.");
        }

        pedido.Status = novoStatus;

        await _pedidoRepository.AtualizarAsync(pedido);

        return PedidoMapper.ParaResponse(pedido);
    }

    public async Task<PedidoResponse> CancelarAsync(int id)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);

        if (pedido is null)
            throw new PedidoNaoEncontradoException(id);

        if (pedido.Status != StatusPedido.Pendente)
        {
            throw new StatusPedidoInvalidoException(
                $"Não é permitido cancelar um pedido " +
                $"com status {pedido.Status}.");
        }

        await _pedidoRepository
            .CancelarComDevolucaoEstoqueAsync(pedido);

        return PedidoMapper.ParaResponse(pedido);
    }
}