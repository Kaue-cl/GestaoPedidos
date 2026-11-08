using GestaoPedidos.API.Data;
using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
}