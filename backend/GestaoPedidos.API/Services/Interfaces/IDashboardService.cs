using GestaoPedidos.API.DTOs.Dashboard;

namespace GestaoPedidos.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> ObterIndicadoresAsync();
}