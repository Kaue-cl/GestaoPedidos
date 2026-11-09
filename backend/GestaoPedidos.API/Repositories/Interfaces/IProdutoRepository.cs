using GestaoPedidos.API.Entities;
using GestaoPedidos.API.DTOs.Produtos;

namespace GestaoPedidos.API.Repositories.Interfaces;

public interface IProdutoRepository
{
    Task<List<Produto>> ListarAsync();

    Task<Produto?> ObterPorIdAsync(int id);

    Task<Produto> AdicionarAsync(Produto produto);

    Task AtualizarAsync(Produto produto);

    Task ExcluirAsync(Produto produto);

    Task<(List<Produto> Dados, int TotalRegistros)> ListarPaginadoAsync(
        FiltroProdutosRequest filtro);
}