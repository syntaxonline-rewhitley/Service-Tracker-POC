using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServiceTicketsController(
    IServiceTicketRepository repository,
    ITechnicianRepository technicianRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType<IEnumerable<ServiceTicketResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tickets = await repository.GetAllAsync(ct);
        return Ok(tickets.Select(ToResponse));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType<IEnumerable<ServiceTicketResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var technician = await technicianRepository.GetByUserIdAsync(userId, ct);
        if (technician is null) return NotFound("No technician record linked to this account.");

        var tickets = await repository.GetByTechnicianAsync(technician.Id, ct);
        return Ok(tickets.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Dispatcher,Technician")]
    [ProducesResponseType<ServiceTicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var ticket = await repository.GetByIdAsync(id, ct);
        return ticket is null ? NotFound() : Ok(ToResponse(ticket));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType<ServiceTicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceTicketRequest request, CancellationToken ct)
    {
        var ticket = new ServiceTicket
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = ServiceTicketStatus.Open,
            Priority = request.Priority,
            CompanyId = request.CompanyId,
            ContactId = request.ContactId,
            TechnicianId = request.TechnicianId,
            ScheduledDate = request.ScheduledDate.HasValue
                ? DateTime.SpecifyKind(request.ScheduledDate.Value, DateTimeKind.Utc)
                : null
        };

        var created = await repository.CreateAsync(ticket, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Dispatcher,Technician")]
    [ProducesResponseType<ServiceTicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceTicketRequest request, CancellationToken ct)
    {
        var updated = new ServiceTicket
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            CompanyId = request.CompanyId,
            ContactId = request.ContactId,
            TechnicianId = request.TechnicianId,
            ScheduledDate = request.ScheduledDate,
            ResolvedAt = request.ResolvedAt,
            ResolutionNotes = request.ResolutionNotes
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

    private static ServiceTicketResponse ToResponse(ServiceTicket t) => new(
        t.Id,
        t.TicketNumber,
        t.Title,
        t.Description,
        t.Status.ToString(),
        t.Priority.ToString(),
        t.CompanyId,
        t.Company?.Name ?? string.Empty,
        t.ContactId,
        t.Contact is null ? null : $"{t.Contact.FirstName} {t.Contact.LastName}",
        t.TechnicianId,
        t.Technician is null ? null : $"{t.Technician.FirstName} {t.Technician.LastName}",
        t.ScheduledDate,
        t.ResolvedAt,
        t.ResolutionNotes,
        t.CreatedAt,
        t.UpdatedAt
    );
}
