using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Entities;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Repositories;
using ServiceTracker.Api.Services;
using Xunit;

namespace ServiceTracker.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<ITechnicianRepository> _technicianRepo;
    private readonly TokenService _tokenService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _technicianRepo = new Mock<ITechnicianRepository>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-chars-long",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        _tokenService = new TokenService(config);
        _sut = new AuthController(_userManager.Object, _technicianRepo.Object, _tokenService);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Different1!", "Admin");

        var result = await _sut.Register(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Passwords do not match.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("SuperAdmin")]
    [InlineData("manager")]
    [InlineData("ADMIN")]
    public async Task Register_ReturnsBadRequest_WhenRoleIsInvalid(string invalidRole)
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", invalidRole);

        var result = await _sut.Register(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Role must be one of: Admin, Dispatcher, Technician.");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Dispatcher")]
    [InlineData("Technician")]
    public async Task Register_ReturnsBadRequest_WhenUserManagerFails(string role)
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", role);
        var identityErrors = new[] { new IdentityError { Description = "Email is already taken." } };
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var result = await _sut.Register(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeAssignableTo<IEnumerable<string>>()
            .Which.Should().ContainSingle().Which.Should().Be("Email is already taken.");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Dispatcher")]
    [InlineData("Technician")]
    public async Task Register_ReturnsOkWithToken_WhenSuccessful(string role)
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", role);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), role))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync([role]);
        _technicianRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.Register(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var token = ok.Value.Should().BeOfType<TokenResponse>().Subject;
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_CreatesUserWithEmailAsUsername()
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", "Admin");
        IdentityUser? captured = null;
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .Callback<IdentityUser, string>((u, _) => captured = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["Admin"]);

        await _sut.Register(request);

        captured.Should().NotBeNull();
        captured!.Email.Should().Be("user@example.com");
        captured.UserName.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Dispatcher")]
    [InlineData("Technician")]
    public async Task Register_AssignsSpecifiedRoleToUser(string role)
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", role);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), role))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync([role]);
        _technicianRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.Register(request);

        _userManager.Verify(m => m.AddToRoleAsync(
            It.Is<IdentityUser>(u => u.Email == "user@example.com"), role), Times.Once);
    }

    [Fact]
    public async Task Register_TokenContainsAssignedRoleClaim()
    {
        var request = new RegisterRequest("admin@example.com", "Password1!", "Password1!", "Admin");
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["Admin"]);

        var result = await _sut.Register(request);

        var tokenResponse = result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeOfType<TokenResponse>().Subject;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(tokenResponse.AccessToken);
        parsed.Claims.Should().Contain(c =>
            c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public async Task Register_LinksTechnicianToUser_WhenTechnicianRoleAndMatchingEmailExists()
    {
        var technicianId = Guid.NewGuid();
        var request = new RegisterRequest("tech@example.com", "Password1!", "Password1!", "Technician");
        var technician = new Technician { Id = technicianId, Email = "tech@example.com" };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .Callback<IdentityUser, string>((u, _) => u.Id = "user-123")
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Technician"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["Technician"]);
        _technicianRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([technician]);

        await _sut.Register(request);

        _technicianRepo.Verify(r =>
            r.LinkUserAsync(technicianId, "user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_DoesNotLinkTechnician_WhenTechnicianRoleButNoMatchingEmailExists()
    {
        var request = new RegisterRequest("tech@example.com", "Password1!", "Password1!", "Technician");
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), "Technician"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(["Technician"]);
        _technicianRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.Register(request);

        _technicianRepo.Verify(r =>
            r.LinkUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Dispatcher")]
    public async Task Register_DoesNotQueryTechnicians_WhenRoleIsNotTechnician(string role)
    {
        var request = new RegisterRequest("user@example.com", "Password1!", "Password1!", role);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), role))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync([role]);

        await _sut.Register(request);

        _technicianRepo.Verify(r =>
            r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        _technicianRepo.Verify(r =>
            r.LinkUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── GetUsers ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_ReturnsOkWithAllUsersAndTheirRoles()
    {
        var user1 = new IdentityUser { Id = "1", Email = "admin@example.com" };
        var user2 = new IdentityUser { Id = "2", Email = "tech@example.com" };
        _userManager.Setup(m => m.Users)
            .Returns(new[] { user1, user2 }.AsQueryable());
        _userManager.Setup(m => m.GetRolesAsync(user1)).ReturnsAsync(["Admin"]);
        _userManager.Setup(m => m.GetRolesAsync(user2)).ReturnsAsync(["Technician"]);

        var result = await _sut.GetUsers();

        var list = result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<UserListItem>>().Subject.ToList();
        list.Should().HaveCount(2);
        list.Should().ContainSingle(u => u.Email == "admin@example.com" && u.Roles.Contains("Admin"));
        list.Should().ContainSingle(u => u.Email == "tech@example.com" && u.Roles.Contains("Technician"));
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithEmptyList_WhenNoUsersExist()
    {
        _userManager.Setup(m => m.Users)
            .Returns(Enumerable.Empty<IdentityUser>().AsQueryable());

        var result = await _sut.GetUsers();

        result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<UserListItem>>()
            .Which.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsers_IncludesMultipleRolesPerUser()
    {
        var user = new IdentityUser { Id = "1", Email = "multi@example.com" };
        _userManager.Setup(m => m.Users)
            .Returns(new[] { user }.AsQueryable());
        _userManager.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["Admin", "Dispatcher"]);

        var result = await _sut.GetUsers();

        var list = result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeAssignableTo<IEnumerable<UserListItem>>().Subject.ToList();
        list.Single().Roles.Should().BeEquivalentTo(["Admin", "Dispatcher"]);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _sut.Login(new LoginRequest("missing@example.com", "Password1!"));

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "WrongPass")).ReturnsAsync(false);

        var result = await _sut.Login(new LoginRequest(user.Email, "WrongPass"));

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ReturnsOkWithToken_WhenCredentialsAreValid()
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);

        var result = await _sut.Login(new LoginRequest(user.Email, "Password1!"));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var token = ok.Value.Should().BeOfType<TokenResponse>().Subject;
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_TokenContainsCorrectSubjectAndEmailClaims()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new IdentityUser { Id = userId, Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);

        var result = await _sut.Login(new LoginRequest(user.Email, "Password1!"));

        var tokenResponse = result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeOfType<TokenResponse>().Subject;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(tokenResponse.AccessToken);
        parsed.Subject.Should().Be(userId);
        parsed.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Dispatcher")]
    [InlineData("Technician")]
    public async Task Login_TokenContainsRoleClaim(string role)
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([role]);

        var result = await _sut.Login(new LoginRequest(user.Email, "Password1!"));

        var tokenResponse = result.Should().BeOfType<OkObjectResult>().Subject
            .Value.Should().BeOfType<TokenResponse>().Subject;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(tokenResponse.AccessToken);
        parsed.Claims.Should().Contain(c =>
            c.Type == "role" && c.Value == role);
    }

    [Fact]
    public async Task Login_DoesNotCheckPassword_WhenUserNotFound()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser?)null);

        await _sut.Login(new LoginRequest("missing@example.com", "Password1!"));

        _userManager.Verify(m =>
            m.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
    }
}
