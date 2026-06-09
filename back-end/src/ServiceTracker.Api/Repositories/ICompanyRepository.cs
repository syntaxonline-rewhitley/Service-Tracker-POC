using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Repositories;

public interface ICompanyRepository
{
    Task<IEnumerable<Company>> GetAllAsync(CancellationToken ct = default);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Company> CreateAsync(Company company, CancellationToken ct = default);
    Task<Company?> UpdateAsync(Guid id, Company updated, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> LinkExists(Guid companyId, Guid contactId, CancellationToken ct = default);
    Task LinkContact(Guid companyId, Guid contactId, CancellationToken ct = default);
    Task<bool> UnlinkContact(Guid companyId, Guid contactId, CancellationToken ct = default);
}
