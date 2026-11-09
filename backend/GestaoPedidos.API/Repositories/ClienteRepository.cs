using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestaoPedidos.API.Data;
using GestaoPedidos.API.DTOs.Clientes;

namespace GestaoPedidos.API.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente> AdicionarAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExisteAsync(string email, int? ignorarClienteId = null)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();

        return await _context.Clientes.AnyAsync(c => 
            c.Email.ToLower() == emailNormalizado &&
            (!ignorarClienteId.HasValue || 
            c.Id != ignorarClienteId.Value)); 
    }

    
    
    public async Task<(List<Cliente> Dados, int TotalRegistros)> ListarPaginadoAsync(FiltroClientesRequest filtro)
    {
        var query = _context.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim();
            query = query.Where(c => c.Nome.Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Email))
        {
            var email = filtro.Email.Trim();
            query = query.Where(c => c.Email.Contains(email));
        }

        var totalRegistros = await query.CountAsync();

        var descendente = filtro.Direcao.Equals(
            "desc", StringComparison.OrdinalIgnoreCase);

        query = filtro.OrdenarPor.ToLowerInvariant() switch
        {
            "data" => descendente
                ? query.OrderByDescending(c => c.DataCriacao).ThenBy(c => c.Id)
                : query.OrderBy(c => c.DataCriacao).ThenBy(c => c.Id),

            _ => descendente
                ? query.OrderByDescending(c => c.Nome).ThenBy(c => c.Id)
                : query.OrderBy(c => c.Nome).ThenBy(c => c.Id)
        };

        var clientes = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return (clientes, totalRegistros);
    }
    public async Task<bool> PossuiPedidosAsync(int clienteId)
    {
        return await _context.Pedidos
            .AnyAsync(p => p.ClienteId == clienteId);
    }
}