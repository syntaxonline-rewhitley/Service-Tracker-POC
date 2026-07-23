using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public interface IServiceTicketRepository
{
    Task<IEnumerable<ServiceTicket>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceTicket?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ServiceTicket>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<IEnumerable<ServiceTicket>> GetByTechnicianAsync(Guid technicianId, CancellationToken ct = default);
    Task<ServiceTicket> CreateAsync(ServiceTicket ticket, CancellationToken ct = default);
    Task<ServiceTicket?> UpdateAsync(Guid id, ServiceTicket updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
