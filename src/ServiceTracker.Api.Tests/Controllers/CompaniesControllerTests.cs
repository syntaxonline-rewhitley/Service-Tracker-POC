using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Tests.Controllers;

public class CompaniesControllerTests
{
    private readonly Mock<ICompanyRepository> _companyRepo = new();
    private readonly Mock<IContactRepository> _contactRepo = new();
    private readonly CompaniesController _sut;

    public CompaniesControllerTests()
    {
        _sut = new CompaniesController(_companyRepo.Object, _contactRepo.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithMappedCompanies()
    {
        var companies = new List<Company>
        {
            MakeCompany("Acme Corp"),
            MakeCompany("Globex Inc")
        };
        _companyRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeAssignableTo<IEnumerable<CompanyResponse>>().Subject.ToList();
        body.Should().HaveCount(2);
        body[0].Name.Should().Be("Acme Corp");
        body[1].Name.Should().Be("Globex Inc");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoCompaniesExist()
    {
        _companyRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Company>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<CompanyResponse>>()
            .Which.Should().BeEmpty();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithCompany_WhenFound()
    {
        var company = MakeCompany("Acme Corp");
        _companyRepo.Setup(r => r.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var result = await _sut.GetById(company.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<CompanyResponse>().Subject;
        body.Id.Should().Be(company.Id);
        body.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _companyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithMappedResponse()
    {
        var request = new CreateCompanyRequest("Acme", "acme@example.com", "555-0100", "123 Main St", "https://acme.com");
        Company? captured = null;
        _companyRepo.Setup(r => r.CreateAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .Callback<Company, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Company c, CancellationToken _) => c);

        var result = await _sut.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(CompaniesController.GetById));
        var body = created.Value.Should().BeOfType<CompanyResponse>().Subject;
        body.Name.Should().Be("Acme");
        body.Email.Should().Be("acme@example.com");
        body.Id.Should().NotBeEmpty();
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedCompany_WhenFound()
    {
        var id = Guid.NewGuid();
        var request = new UpdateCompanyRequest("Updated Name", null, null, null, null);
        var updated = new Company { Id = id, Name = "Updated Name" };
        _companyRepo.Setup(r => r.UpdateAsync(id, It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.Update(id, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<CompanyResponse>().Which.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        _companyRepo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Company>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var result = await _sut.Update(Guid.NewGuid(), new UpdateCompanyRequest("X", null, null, null, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        _companyRepo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        _companyRepo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- GetContacts ---

    [Fact]
    public async Task GetContacts_ReturnsOkWithContacts_WhenCompanyExists()
    {
        var companyId = Guid.NewGuid();
        var contacts = new List<Contact> { MakeContact("Alice", "Smith") };
        _companyRepo.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Company { Id = companyId, Name = "Acme" });
        _contactRepo.Setup(r => r.GetCompanyContacts(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);

        var result = await _sut.GetContacts(companyId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ContactResponse>>()
            .Which.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetContacts_ReturnsNotFound_WhenCompanyMissing()
    {
        _companyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var result = await _sut.GetContacts(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- LinkContact ---

    [Fact]
    public async Task LinkContact_ReturnsNoContent_WhenSuccessful()
    {
        var companyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        _companyRepo.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Company { Id = companyId, Name = "Acme" });
        _contactRepo.Setup(r => r.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeContact("Alice", "Smith"));
        _companyRepo.Setup(r => r.LinkExists(companyId, contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _companyRepo.Setup(r => r.LinkContact(companyId, contactId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.LinkContact(companyId, contactId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task LinkContact_ReturnsNotFound_WhenCompanyMissing()
    {
        _companyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var result = await _sut.LinkContact(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task LinkContact_ReturnsNotFound_WhenContactMissing()
    {
        var companyId = Guid.NewGuid();
        _companyRepo.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Company { Id = companyId, Name = "Acme" });
        _contactRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var result = await _sut.LinkContact(companyId, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task LinkContact_ReturnsConflict_WhenAlreadyLinked()
    {
        var companyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        _companyRepo.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Company { Id = companyId, Name = "Acme" });
        _contactRepo.Setup(r => r.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeContact("Alice", "Smith"));
        _companyRepo.Setup(r => r.LinkExists(companyId, contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.LinkContact(companyId, contactId, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    // --- UnlinkContact ---

    [Fact]
    public async Task UnlinkContact_ReturnsNoContent_WhenSuccessful()
    {
        _companyRepo.Setup(r => r.UnlinkContact(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UnlinkContact(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UnlinkContact_ReturnsNotFound_WhenLinkMissing()
    {
        _companyRepo.Setup(r => r.UnlinkContact(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UnlinkContact(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Helpers ---

    private static Company MakeCompany(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Contact MakeContact(string first, string last) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = first,
        LastName = last,
        Email = $"{first.ToLower()}.{last.ToLower()}@example.com",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
