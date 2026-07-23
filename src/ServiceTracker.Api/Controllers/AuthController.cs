using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;
using ServiceTracker.Api.Services;

namespace ServiceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ITechnicianRepository technicianRepository,
    TokenService tokenService) : ControllerBase
{
    private static readonly string[] ValidRoles = ["Admin", "Dispatcher", "Technician"];

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        if (!ValidRoles.Contains(request.Role))
            return BadRequest($"Role must be one of: {string.Join(", ", ValidRoles)}.");

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, request.Role);

        // Auto-link technician entity to the new user account
        if (request.Role == "Technician")
        {
            var technician = (await technicianRepository.GetAllAsync())
                .FirstOrDefault(t => t.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (technician is not null)
                await technicianRepository.LinkUserAsync(technician.Id, user.Id);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(tokenService.GenerateToken(user, roles));
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IEnumerable<UserListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var users = userManager.Users.ToList();
        var result = new List<UserListItem>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(new UserListItem(u.Id, u.Email!, roles));
        }
        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");
        if (id == currentUserId)
            return BadRequest("You cannot delete your own account.");

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        return Ok(tokenService.GenerateToken(user, roles));
    }
}
