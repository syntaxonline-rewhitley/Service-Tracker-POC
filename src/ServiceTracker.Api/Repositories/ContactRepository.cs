using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public class ContactRepository(ServiceTrackerDbContext db) : IContactRepository
{
    public async Task<IEnumerable<Contact>> GetAllAsync(CancellationToken ct = default) =>
        await db.Contacts.AsNoTracking().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync(ct);

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Contact> CreateAsync(Contact contact, CancellationToken ct = default)
    {
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;
        db.Contacts.Add(contact);
        await db.SaveChangesAsync(ct);
        return contact;
    }

    public async Task<Contact?> UpdateAsync(Guid id, Contact updated, CancellationToken ct = default)
    {
        var existing = await db.Contacts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing is null) return null;

        existing.FirstName = updated.FirstName;
        existing.LastName = updated.LastName;
        existing.Email = updated.Email;
        existing.Phone = updated.Phone;
        existing.Address = updated.Address;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rows = await db.Contacts.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }

    public async Task<IEnumerable<Contact>> GetCompanyContacts(Guid companyId, CancellationToken ct = default)
    {
        var contacts = await db.CompanyContacts
            .Where(cc => cc.CompanyId == companyId)
            .Select(cc => cc.Contact)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .AsNoTracking()
            .ToListAsync(ct);

        return contacts;
    }
}
