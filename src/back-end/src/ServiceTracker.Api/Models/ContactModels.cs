using System.ComponentModel.DataAnnotations;

namespace ServiceTracker.Api.Models;

public record ContactResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateContactRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(500)] string? Address
);

public record UpdateContactRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [MaxLength(30)] string? Phone,
    [MaxLength(500)] string? Address
);
