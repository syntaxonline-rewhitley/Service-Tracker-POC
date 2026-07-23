using ServiceTracker.Api.Models;

namespace ServiceTracker.Api.Repositories;

public interface IDashboardRepository
{
    Task<DashboardStats> GetStatsAsync(CancellationToken ct = default);
    Task<TechnicianDashboardStats?> GetTechnicianStatsAsync(string userId, CancellationToken ct = default);
}
