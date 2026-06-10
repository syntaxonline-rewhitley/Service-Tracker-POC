using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ServiceTracker.Api.Controllers;
using ServiceTracker.Api.Models;
using ServiceTracker.Api.Services;
using Xunit;

namespace ServiceTracker.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly TokenService _tokenService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-chars-long",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        _tokenService = new TokenService(config);
        _sut = new AuthController(_userManager.Object, _tokenService);
    }

    // --- Register ---

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Different1");

        var result = await _sut.Register(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().Be("Passwords do not match.");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserManagerFails()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Password1");
        var identityErrors = new[] { new IdentityError { Description = "Email is already taken." } };
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var result = await _sut.Register(request);

        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errors = bad.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        errors.Should().ContainSingle().Which.Should().Be("Email is already taken.");
    }

    [Fact]
    public async Task Register_ReturnsOkWithToken_WhenSuccessful()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Password1");
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.Register(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var token = ok.Value.Should().BeOfType<TokenResponse>().Subject;
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_CreatesUserWithEmailAsUsername()
    {
        var request = new RegisterRequest("user@example.com", "Password1", "Password1");
        IdentityUser? captured = null;
        _userManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), request.Password))
            .Callback<IdentityUser, string>((u, _) => captured = u)
            .ReturnsAsync(IdentityResult.Success);

        await _sut.Register(request);

        captured.Should().NotBeNull();
        captured!.Email.Should().Be("user@example.com");
        captured.UserName.Should().Be("user@example.com");
    }

    // --- Login ---

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _sut.Login(new LoginRequest("missing@example.com", "Password1"));

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "WrongPass"))
            .ReturnsAsync(false);

        var result = await _sut.Login(new LoginRequest(user.Email, "WrongPass"));

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ReturnsOkWithToken_WhenCredentialsAreValid()
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1"))
            .ReturnsAsync(true);

        var result = await _sut.Login(new LoginRequest(user.Email, "Password1"));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var token = ok.Value.Should().BeOfType<TokenResponse>().Subject;
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_TokenContainsCorrectClaims()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new IdentityUser { Id = userId, Email = "user@example.com" };
        _userManager.Setup(m => m.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1"))
            .ReturnsAsync(true);

        var result = await _sut.Login(new LoginRequest(user.Email, "Password1"));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var tokenResponse = ok.Value.Should().BeOfType<TokenResponse>().Subject;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(tokenResponse.AccessToken);
        parsed.Subject.Should().Be(userId);
        parsed.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
    }
}
