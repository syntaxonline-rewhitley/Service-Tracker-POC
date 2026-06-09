using System.ComponentModel.DataAnnotations;

namespace ServiceTracker.Api.Models;

public record CompanyResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Address,
    string? Website,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateCompanyRequest(
    [Required, MaxLength(200)] string Name,
    [EmailAddress, MaxLength(254)] string? Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(500)] string? Address,
    [Url, MaxLength(2048)] string? Website
);

public record UpdateCompanyRequest(
    [Required, MaxLength(200)] string Name,
    [EmailAddress, MaxLength(254)] string? Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(500)] string? Address,
    [Url, MaxLength(2048)] string? Website
);
