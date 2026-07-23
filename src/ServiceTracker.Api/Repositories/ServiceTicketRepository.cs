using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public class ServiceTicketRepository(ServiceTrackerDbContext db) : IServiceTicketRepository
{
    private IQueryable<ServiceTicket> WithIncludes() =>
        db.ServiceTickets
            .Include(t => t.Company)
            .Include(t => t.Contact)
            .Include(t => t.Technician);

    public async Task<IEnumerable<ServiceTicket>> GetAllAsync(CancellationToken ct = default) =>
        await WithIncludes().AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public async Task<ServiceTicket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await WithIncludes().AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IEnumerable<ServiceTicket>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default) =>
        await WithIncludes().AsNoTracking()
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<ServiceTicket>> GetByTechnicianAsync(Guid technicianId, CancellationToken ct = default) =>
        await WithIncludes().AsNoTracking()
            .Where(t => t.TechnicianId == technicianId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<ServiceTicket> CreateAsync(ServiceTicket ticket, CancellationToken ct = default)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.TicketNumber = await GenerateTicketNumberAsync(ct);

        db.ServiceTickets.Add(ticket);
        await db.SaveChangesAsync(ct);

        await db.Entry(ticket).Reference(t => t.Company).LoadAsync(ct);
        await db.Entry(ticket).Reference(t => t.Contact).LoadAsync(ct);
        await db.Entry(ticket).Reference(t => t.Technician).LoadAsync(ct);

        return ticket;
    }

    public async Task<ServiceTicket?> UpdateAsync(Guid id, ServiceTicket updated, CancellationToken ct = default)
    {
        var existing = await WithIncludes().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (existing is null) return null;

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Status = updated.Status;
        existing.Priority = updated.Priority;
        existing.CompanyId = updated.CompanyId;
        existing.ContactId = updated.ContactId;
        existing.TechnicianId = updated.TechnicianId;
        existing.ScheduledDate = updated.ScheduledDate;
        existing.ResolvedAt = updated.ResolvedAt;
        existing.ResolutionNotes = updated.ResolutionNotes;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        await db.Entry(existing).Reference(t => t.Company).LoadAsync(ct);
        await db.Entry(existing).Reference(t => t.Contact).LoadAsync(ct);
        await db.Entry(existing).Reference(t => t.Technician).LoadAsync(ct);

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rows = await db.ServiceTickets.Where(t => t.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }

    private async Task<string> GenerateTicketNumberAsync(CancellationToken ct)
    {
        var count = await db.ServiceTickets.CountAsync(ct);
        return $"TKT-{(count + 1):D6}";
    }
}
