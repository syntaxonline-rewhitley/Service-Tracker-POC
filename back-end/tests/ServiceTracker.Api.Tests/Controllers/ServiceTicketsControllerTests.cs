using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Tests.Controllers;

public class ServiceTicketsControllerTests
{
    private readonly Mock<IServiceTicketRepository> _repo = new();
    private readonly Mock<ITechnicianRepository> _technicianRepo = new();
    private readonly ServiceTicketsController _sut;

    public ServiceTicketsControllerTests()
    {
        _sut = new ServiceTicketsController(_repo.Object, _technicianRepo.Object);
    }

    private static ServiceTicketsController CreateSutWithUser(
        Mock<IServiceTicketRepository> repo,
        Mock<ITechnicianRepository> technicianRepo,
        string userId)
    {
        var controller = new ServiceTicketsController(repo.Object, technicianRepo.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "test"))
            }
        };
        return controller;
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithMappedTickets()
    {
        var tickets = new List<ServiceTicket>
        {
            MakeTicket("Fix printer"),
            MakeTicket("Network outage")
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeAssignableTo<IEnumerable<ServiceTicketResponse>>().Subject.ToList();
        body.Should().HaveCount(2);
        body[0].Title.Should().Be("Fix printer");
        body[1].Title.Should().Be("Network outage");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTicketsExist()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ServiceTicket>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ServiceTicketResponse>>()
            .Which.Should().BeEmpty();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithTicket_WhenFound()
    {
        var ticket = MakeTicket("Fix printer");
        _repo.Setup(r => r.GetByIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var result = await _sut.GetById(ticket.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ServiceTicketResponse>().Subject;
        body.Id.Should().Be(ticket.Id);
        body.Title.Should().Be("Fix printer");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceTicket?)null);

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithMappedResponse()
    {
        var companyId = Guid.NewGuid();
        var request = new CreateServiceTicketRequest(
            "Fix printer",
            "Printer is broken",
            companyId,
            null,
            null,
            ServiceTicketPriority.High);

        _repo.Setup(r => r.CreateAsync(It.IsAny<ServiceTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceTicket t, CancellationToken _) =>
            {
                t.Company = new Company { Id = companyId, Name = "Acme" };
                return t;
            });

        var result = await _sut.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ServiceTicketsController.GetById));
        var body = created.Value.Should().BeOfType<ServiceTicketResponse>().Subject;
        body.Title.Should().Be("Fix printer");
        body.Status.Should().Be("Open");
        body.Priority.Should().Be("High");
        body.CompanyId.Should().Be(companyId);
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_SetsStatusToOpen_Automatically()
    {
        var companyId = Guid.NewGuid();
        ServiceTicket? captured = null;
        _repo.Setup(r => r.CreateAsync(It.IsAny<ServiceTicket>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceTicket, CancellationToken>((t, _) => captured = t)
            .ReturnsAsync((ServiceTicket t, CancellationToken _) =>
            {
                t.Company = new Company { Id = companyId, Name = "Acme" };
                return t;
            });

        await _sut.Create(new CreateServiceTicketRequest("Title", null, companyId, null, null), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(ServiceTicketStatus.Open);
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedTicket_WhenFound()
    {
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var request = new UpdateServiceTicketRequest(
            "Updated Title",
            null,
            companyId,
            null,
            null,
            ServiceTicketStatus.InProgress,
            ServiceTicketPriority.Medium);
        var updated = MakeTicket("Updated Title");
        updated.Id = id;
        updated.Status = ServiceTicketStatus.InProgress;
        updated.Company = new Company { Id = companyId, Name = "Acme" };

        _repo.Setup(r => r.UpdateAsync(id, It.IsAny<ServiceTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.Update(id, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ServiceTicketResponse>().Subject;
        body.Title.Should().Be("Updated Title");
        body.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<ServiceTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceTicket?)null);

        var result = await _sut.Update(
            Guid.NewGuid(),
            new UpdateServiceTicketRequest("T", null, Guid.NewGuid(), null, null),
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- GetMine ---

    [Fact]
    public async Task GetMine_ReturnsOkWithTechnicianTickets_WhenLinkedTechnicianExists()
    {
        var userId = Guid.NewGuid().ToString();
        var technicianId = Guid.NewGuid();
        var technician = new Technician { Id = technicianId, UserId = userId, Email = "tech@example.com" };
        var tickets = new List<ServiceTicket> { MakeTicket("Fix server"), MakeTicket("Replace cable") };

        _technicianRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);
        _repo.Setup(r => r.GetByTechnicianAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        var sut = CreateSutWithUser(_repo, _technicianRepo, userId);
        var result = await sut.GetMine(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ServiceTicketResponse>>()
            .Which.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMine_ReturnsOkWithEmptyList_WhenTechnicianHasNoTickets()
    {
        var userId = Guid.NewGuid().ToString();
        var technicianId = Guid.NewGuid();
        var technician = new Technician { Id = technicianId, UserId = userId, Email = "tech@example.com" };

        _technicianRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);
        _repo.Setup(r => r.GetByTechnicianAsync(technicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSutWithUser(_repo, _technicianRepo, userId);
        var result = await sut.GetMine(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ServiceTicketResponse>>()
            .Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMine_ReturnsNotFound_WhenNoTechnicianLinkedToUser()
    {
        var userId = Guid.NewGuid().ToString();
        _technicianRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        var sut = CreateSutWithUser(_repo, _technicianRepo, userId);
        var result = await sut.GetMine(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("No technician record linked to this account.");
    }

    [Fact]
    public async Task GetMine_ReturnsUnauthorized_WhenUserClaimIsMissing()
    {
        var controller = new ServiceTicketsController(_repo.Object, _technicianRepo.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await controller.GetMine(CancellationToken.None);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // --- Helpers ---

    private static ServiceTicket MakeTicket(string title) => new()
    {
        Id = Guid.NewGuid(),
        TicketNumber = "TKT-001",
        Title = title,
        Status = ServiceTicketStatus.Open,
        Priority = ServiceTicketPriority.Medium,
        CompanyId = Guid.NewGuid(),
        Company = new Company { Id = Guid.NewGuid(), Name = "Acme" },
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
