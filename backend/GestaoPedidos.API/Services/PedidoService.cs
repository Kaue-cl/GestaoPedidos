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

    public Task<PedidoResponse> ObterPorIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<PedidoResponse>> ListarAsync(
        int pagina,
        int tamanhoPagina)
    {
        throw new NotImplementedException();
    }

    public Task<int> ContarAsync()
    {
        throw new NotImplementedException();
    }

    public Task<PedidoResponse> AtualizarStatusAsync(
        int id,
        AtualizarStatusPedidoRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PedidoResponse> CancelarAsync(int id)
    {
        throw new NotImplementedException();
    }
}