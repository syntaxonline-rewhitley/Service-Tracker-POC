using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync(CancellationToken ct = default);
    Task<Technician?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Technician?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Technician> CreateAsync(Technician technician, CancellationToken ct = default);
    Task<Technician?> UpdateAsync(Guid id, Technician updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task LinkUserAsync(Guid technicianId, string userId, CancellationToken ct = default);
}
