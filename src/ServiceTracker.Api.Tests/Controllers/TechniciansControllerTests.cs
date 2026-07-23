using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Tests.Controllers;

public class TechniciansControllerTests
{
    private readonly Mock<ITechnicianRepository> _repo = new();
    private readonly TechniciansController _sut;

    public TechniciansControllerTests()
    {
        _sut = new TechniciansController(_repo.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithMappedTechnicians()
    {
        var technicians = new List<Technician>
        {
            MakeTechnician("Alice", "Smith"),
            MakeTechnician("Bob", "Jones")
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(technicians);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeAssignableTo<IEnumerable<TechnicianResponse>>().Subject.ToList();
        body.Should().HaveCount(2);
        body[0].FirstName.Should().Be("Alice");
        body[1].FirstName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTechniciansExist()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Technician>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<TechnicianResponse>>()
            .Which.Should().BeEmpty();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithTechnician_WhenFound()
    {
        var technician = MakeTechnician("Alice", "Smith");
        _repo.Setup(r => r.GetByIdAsync(technician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(technician);

        var result = await _sut.GetById(technician.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<TechnicianResponse>().Subject;
        body.Id.Should().Be(technician.Id);
        body.FirstName.Should().Be("Alice");
        body.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithMappedResponse()
    {
        var request = new CreateTechnicianRequest("Alice", "Smith", "alice@example.com", "555-0100", "Networking");
        _repo.Setup(r => r.CreateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician t, CancellationToken _) => t);

        var result = await _sut.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TechniciansController.GetById));
        var body = created.Value.Should().BeOfType<TechnicianResponse>().Subject;
        body.FirstName.Should().Be("Alice");
        body.LastName.Should().Be("Smith");
        body.Email.Should().Be("alice@example.com");
        body.Specialization.Should().Be("Networking");
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_SetsIsActiveToTrue_Automatically()
    {
        Technician? captured = null;
        _repo.Setup(r => r.CreateAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Callback<Technician, CancellationToken>((t, _) => captured = t)
            .ReturnsAsync((Technician t, CancellationToken _) => t);

        await _sut.Create(new CreateTechnicianRequest("Alice", "Smith", "alice@example.com", null, null), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.IsActive.Should().BeTrue();
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedTechnician_WhenFound()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTechnicianRequest("Updated", "Name", "updated@example.com", null, null, false);
        var updated = new Technician { Id = id, FirstName = "Updated", LastName = "Name", Email = "updated@example.com", IsActive = false };
        _repo.Setup(r => r.UpdateAsync(id, It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.Update(id, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<TechnicianResponse>().Subject;
        body.FirstName.Should().Be("Updated");
        body.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technician?)null);

        var result = await _sut.Update(
            Guid.NewGuid(),
            new UpdateTechnicianRequest("X", "Y", "x@y.com", null, null, true),
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

    // --- Helpers ---

    private static Technician MakeTechnician(string first, string last) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = first,
        LastName = last,
        Email = $"{first.ToLower()}.{last.ToLower()}@example.com",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
