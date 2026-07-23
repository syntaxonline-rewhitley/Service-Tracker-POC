using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync(CancellationToken ct = default);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contact> CreateAsync(Contact contact, CancellationToken ct = default);
    Task<Contact?> UpdateAsync(Guid id, Contact updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Contact>> GetCompanyContacts(Guid companyId, CancellationToken ct = default);

}
