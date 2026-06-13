using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardRepository dashboardRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Dispatcher")]
    [ProducesResponseType<DashboardStats>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await dashboardRepository.GetStatsAsync(ct);
        return Ok(stats);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType<TechnicianDashboardStats>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyStats(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var stats = await dashboardRepository.GetTechnicianStatsAsync(userId, ct);
        if (stats is null) return NotFound("No technician profile is linked to this account.");

        return Ok(stats);
    }
}
