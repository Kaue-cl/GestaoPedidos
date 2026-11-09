using GestaoPedidos.API.DTOs.Produtos;
using GestaoPedidos.API.DTOs.Common;

namespace GestaoPedidos.API.Services.Interfaces;

public interface IProdutoService
{
    Task<List<ProdutoResponse>> ListarAsync();

    Task<ProdutoResponse> ObterPorIdAsync(int id);

    Task<ProdutoResponse> AdicionarAsync(
        CriarProdutoRequest request);

    Task<ProdutoResponse> AtualizarAsync(
        int id,
        AtualizarProdutoRequest request);

    Task ExcluirAsync(int id);

    Task<int> ConsultarEstoqueAsync(int id);

    Task<PaginacaoResponse<ProdutoResponse>> ListarPaginadoAsync(
        FiltroProdutosRequest filtro);
}