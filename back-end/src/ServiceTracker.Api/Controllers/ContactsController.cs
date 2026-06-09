using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController(IContactRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ContactResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var contacts = await repository.GetAllAsync(ct);
        return Ok(contacts.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var contact = await repository.GetByIdAsync(id, ct);
        return contact is null ? NotFound() : Ok(ToResponse(contact));
    }

    [HttpPost]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken ct)
    {
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        var created = await repository.CreateAsync(contact, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request, CancellationToken ct)
    {
        var updated = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
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

    private static ContactResponse ToResponse(Contact c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Address, c.CreatedAt, c.UpdatedAt);
}
