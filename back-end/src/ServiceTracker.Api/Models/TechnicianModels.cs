using System.ComponentModel.DataAnnotations;

namespace ServiceTracker.Api.Models;

public record TechnicianResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Specialization,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTechnicianRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(200)] string? Specialization
);

public record UpdateTechnicianRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(200)] string? Specialization,
    bool IsActive
);
