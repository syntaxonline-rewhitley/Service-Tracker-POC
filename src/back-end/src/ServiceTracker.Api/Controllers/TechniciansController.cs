using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TechniciansController(ITechnicianRepository repository) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType<IEnumerable<TechnicianResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var technicians = await repository.GetAllAsync(ct);
        return Ok(technicians.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType<TechnicianResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var technician = await repository.GetByIdAsync(id, ct);
        return technician is null ? NotFound() : Ok(ToResponse(technician));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TechnicianResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTechnicianRequest request, CancellationToken ct)
    {
        var technician = new Technician
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            IsActive = true
        };

        var created = await repository.CreateAsync(technician, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TechnicianResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTechnicianRequest request, CancellationToken ct)
    {
        var updated = new Technician
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            IsActive = request.IsActive
        };

        var result = await repository.UpdateAsync(id, updated, ct);
        return result is null ? NotFound() : Ok(ToResponse(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await repository.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    private static TechnicianResponse ToResponse(Technician t) =>
        new(t.Id, t.FirstName, t.LastName, t.Email, t.Phone, t.Specialization, t.IsActive, t.CreatedAt, t.UpdatedAt);
}
