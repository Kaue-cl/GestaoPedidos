using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}