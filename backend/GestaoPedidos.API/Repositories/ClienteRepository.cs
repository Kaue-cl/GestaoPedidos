using GestaoPedidos.API.Entities;
using GestaoPedidos.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using GestaoPedidos.API.Data;

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
}