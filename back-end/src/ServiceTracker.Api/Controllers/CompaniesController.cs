using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CompaniesController(ICompanyRepository repository, IContactRepository contactRepository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<CompanyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var companies = await repository.GetAllAsync(ct);
        return Ok(companies.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var company = await repository.GetByIdAsync(id, ct);
        return company is null ? NotFound() : Ok(ToResponse(company));
    }

    [HttpPost]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Website = request.Website
        };

        var created = await repository.CreateAsync(company, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var updated = new Company
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Website = request.Website
        };

        var result = await repository.UpdateAsync(id, updated, ct);
        return result is null ? NotFound() : Ok(ToResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await repository.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{companyId:guid}/contacts")]
    [ProducesResponseType<IEnumerable<ContactResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContacts(Guid companyId, CancellationToken ct)
    {
        var company = await repository.GetByIdAsync(companyId, ct);
        if (company is null) return NotFound();

        var contacts = await contactRepository.GetCompanyContacts(companyId, ct);

        return Ok(contacts.Select(ToResponse));
    }

    [HttpPost("{companyId:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkContact(Guid companyId, Guid contactId, CancellationToken ct)
    {
        var company = await repository.GetByIdAsync(companyId, ct);
        if (company is null) return NotFound("Company not found.");

        var contact = await contactRepository.GetByIdAsync(contactId, ct);
        if (contact is null) return NotFound("Contact not found.");

        var alreadyLinked = await repository.LinkExists(companyId, contactId, ct);
        if (alreadyLinked) return Conflict("Contact is already linked to this company.");

        await repository.LinkContact(companyId, contactId, ct);

        return NoContent();
    }

    [HttpDelete("{companyId:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkContact(Guid companyId, Guid contactId, CancellationToken ct)
    {
        var success = await repository.UnlinkContact(companyId, contactId, ct);

        return success ? NoContent() : NotFound();
    }

    private static ContactResponse ToResponse(Contact c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Address, c.CreatedAt, c.UpdatedAt);

    private static CompanyResponse ToResponse(Company c) =>
        new(c.Id, c.Name, c.Email, c.Phone, c.Address, c.Website, c.CreatedAt, c.UpdatedAt);
}
