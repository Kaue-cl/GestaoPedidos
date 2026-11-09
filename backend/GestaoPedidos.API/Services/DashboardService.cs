using GestaoPedidos.API.DTOs.Dashboard;
using GestaoPedidos.API.Repositories.Interfaces;
using GestaoPedidos.API.Services.Interfaces;

namespace GestaoPedidos.API.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardResponse> ObterIndicadoresAsync()
    {
        return await _repository.ObterIndicadoresAsync();
    }
}