using System.ComponentModel.DataAnnotations;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Models;

public record ServiceTicketResponse(
    Guid Id,
    string TicketNumber,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid CompanyId,
    string CompanyName,
    Guid? ContactId,
    string? ContactName,
    Guid? TechnicianId,
    string? TechnicianName,
    DateTime? ScheduledDate,
    DateTime? ResolvedAt,
    string? ResolutionNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateServiceTicketRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    [Required] Guid CompanyId,
    Guid? ContactId,
    Guid? TechnicianId,
    ServiceTicketPriority Priority = ServiceTicketPriority.Medium,
    DateTime? ScheduledDate = null
);

public record UpdateServiceTicketRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    [Required] Guid CompanyId,
    Guid? ContactId,
    Guid? TechnicianId,
    ServiceTicketStatus Status = ServiceTicketStatus.Open,
    ServiceTicketPriority Priority = ServiceTicketPriority.Medium,
    DateTime? ScheduledDate = null,
    DateTime? ResolvedAt = null,
    [MaxLength(2000)] string? ResolutionNotes = null
);
