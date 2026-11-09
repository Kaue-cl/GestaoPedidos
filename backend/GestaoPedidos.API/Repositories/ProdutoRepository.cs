using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestaoPedidos.API.DTOs.Produtos;

namespace GestaoPedidos.API.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> ListarAsync()
    {
        return await _context.Produtos
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Produto> AdicionarAsync(Produto produto)
    {
        await _context.Produtos.AddAsync(produto);

        await _context.SaveChangesAsync();

        return produto;
    }

    public async Task AtualizarAsync(Produto produto)
    {
        _context.Produtos.Update(produto);

        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(Produto produto)
    {
        _context.Produtos.Remove(produto);

        await _context.SaveChangesAsync();
    }
    
    public async Task<(List<Produto> Dados, int TotalRegistros)> ListarPaginadoAsync(FiltroProdutosRequest filtro)
    {
        var query = _context.Produtos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim();
            query = query.Where(p => p.Nome.Contains(nome));
        }

        if (filtro.Ativo.HasValue)
        {
            query = query.Where(p => p.Ativo == filtro.Ativo.Value);
        }

        var totalRegistros = await query.CountAsync();

        var descendente = filtro.Direcao.Equals(
            "desc", StringComparison.OrdinalIgnoreCase);

        query = filtro.OrdenarPor.ToLowerInvariant() switch
        {
            "preco" => descendente
                ? query.OrderByDescending(p => p.Preco).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Preco).ThenBy(p => p.Id),

            "estoque" => descendente
                ? query.OrderByDescending(p => p.EstoqueAtual).ThenBy(p => p.Id)
                : query.OrderBy(p => p.EstoqueAtual).ThenBy(p => p.Id),

            _ => descendente
                ? query.OrderByDescending(p => p.Nome).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Nome).ThenBy(p => p.Id)
        };

        var produtos = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return (produtos, totalRegistros);
    }
}