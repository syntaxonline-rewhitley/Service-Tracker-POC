using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public class CompanyRepository(ServiceTrackerDbContext db) : ICompanyRepository
{
    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken ct = default) =>
        await db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Company> CreateAsync(Company company, CancellationToken ct = default)
    {
        company.CreatedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;
        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);
        return company;
    }

    public async Task<Company?> UpdateAsync(Guid id, Company updated, CancellationToken ct = default)
    {
        var existing = await db.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.Email = updated.Email;
        existing.Phone = updated.Phone;
        existing.Address = updated.Address;
        existing.Website = updated.Website;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rows = await db.Companies.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }

    public async Task LinkContact(Guid companyId, Guid contactId, CancellationToken ct = default)
    {
        db.CompanyContacts.Add(new CompanyContact
        {
            CompanyId = companyId,
            ContactId = contactId,
            LinkedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

    }

    public async Task<bool> UnlinkContact(Guid companyId, Guid contactId, CancellationToken ct = default)
    {
        var rows = await db.CompanyContacts
        .Where(cc => cc.CompanyId == companyId && cc.ContactId == contactId)
        .ExecuteDeleteAsync(ct);

        return rows > 0;
    }

    public async Task<bool> LinkExists(Guid companyId, Guid contactId, CancellationToken ct = default)
    {
        var alreadyLinked = await db.CompanyContacts
            .AnyAsync(cc => cc.CompanyId == companyId && cc.ContactId == contactId, ct);
        return alreadyLinked;
    }
}
