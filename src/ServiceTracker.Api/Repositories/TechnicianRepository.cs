using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public class TechnicianRepository(ServiceTrackerDbContext db) : ITechnicianRepository
{
    public async Task<IEnumerable<Technician>> GetAllAsync(CancellationToken ct = default) =>
        await db.Technicians.AsNoTracking().OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToListAsync(ct);

    public async Task<Technician?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Technicians.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Technician?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await db.Technicians.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId, ct);

    public async Task<Technician> CreateAsync(Technician technician, CancellationToken ct = default)
    {
        technician.CreatedAt = DateTime.UtcNow;
        technician.UpdatedAt = DateTime.UtcNow;
        db.Technicians.Add(technician);
        await db.SaveChangesAsync(ct);
        return technician;
    }

    public async Task<Technician?> UpdateAsync(Guid id, Technician updated, CancellationToken ct = default)
    {
        var existing = await db.Technicians.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (existing is null) return null;

        existing.FirstName = updated.FirstName;
        existing.LastName = updated.LastName;
        existing.Email = updated.Email;
        existing.Phone = updated.Phone;
        existing.Specialization = updated.Specialization;
        existing.IsActive = updated.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rows = await db.Technicians.Where(t => t.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }

    public async Task LinkUserAsync(Guid technicianId, string userId, CancellationToken ct = default)
    {
        await db.Technicians
            .Where(t => t.Id == technicianId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UserId, userId), ct);
    }
}
