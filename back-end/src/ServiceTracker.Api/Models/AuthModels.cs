using System.ComponentModel.DataAnnotations;

namespace ServiceTracker.Api.Models;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string ConfirmPassword,
    [Required] string Role
);

public record TokenResponse(
    string AccessToken,
    DateTime ExpiresAt
);

public record UserListItem(
    string Id,
    string Email,
    IList<string> Roles
);
