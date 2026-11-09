using GestaoPedidos.API.DTOs.Dashboard;

namespace GestaoPedidos.API.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardResponse> ObterIndicadoresAsync();
}