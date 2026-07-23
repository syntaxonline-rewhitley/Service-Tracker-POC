using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;

namespace ServiceTracker.Api.Tests.Controllers;

public class ContactsControllerTests
{
    private readonly Mock<IContactRepository> _repo = new();
    private readonly ContactsController _sut;

    public ContactsControllerTests()
    {
        _sut = new ContactsController(_repo.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithMappedContacts()
    {
        var contacts = new List<Contact>
        {
            MakeContact("Alice", "Smith"),
            MakeContact("Bob", "Jones")
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeAssignableTo<IEnumerable<ContactResponse>>().Subject.ToList();
        body.Should().HaveCount(2);
        body[0].FirstName.Should().Be("Alice");
        body[1].FirstName.Should().Be("Bob");
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoContactsExist()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Contact>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ContactResponse>>()
            .Which.Should().BeEmpty();
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithContact_WhenFound()
    {
        var contact = MakeContact("Alice", "Smith");
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var result = await _sut.GetById(contact.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ContactResponse>().Subject;
        body.Id.Should().Be(contact.Id);
        body.FirstName.Should().Be("Alice");
        body.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var result = await _sut.GetById(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithMappedResponse()
    {
        var request = new CreateContactRequest("Alice", "Smith", "alice@example.com", "555-0100", "123 Main St");
        _repo.Setup(r => r.CreateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.Create(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ContactsController.GetById));
        var body = created.Value.Should().BeOfType<ContactResponse>().Subject;
        body.FirstName.Should().Be("Alice");
        body.LastName.Should().Be("Smith");
        body.Email.Should().Be("alice@example.com");
        body.Id.Should().NotBeEmpty();
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedContact_WhenFound()
    {
        var id = Guid.NewGuid();
        var request = new UpdateContactRequest("Updated", "Name", "updated@example.com", null, null);
        var updated = new Contact { Id = id, FirstName = "Updated", LastName = "Name", Email = "updated@example.com" };
        _repo.Setup(r => r.UpdateAsync(id, It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.Update(id, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ContactResponse>().Subject;
        body.FirstName.Should().Be("Updated");
        body.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var result = await _sut.Update(Guid.NewGuid(), new UpdateContactRequest("X", "Y", "x@y.com", null, null), CancellationToken.None);

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
