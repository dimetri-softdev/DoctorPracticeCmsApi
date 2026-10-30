using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DoctorPracticeCmsApi.Models;
using DoctorPracticeCmsApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DoctorPracticeCmsApi.Tests;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public async Task CreateTokenAsync_IncludesAdminRole()
    {
        var user = new AppUser
        {
            Id = "admin-123",
            Email = "admin@example.com",
            FullName = "Practice Admin"
        };
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        var service = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                Key = "unit-test-signing-key-with-at-least-32-bytes",
                Issuer = "DoctorPracticeCmsApi.Tests",
                Audience = "DoctorPracticeCmsApi.Tests",
                ExpirationMinutes = 15
            }),
            userManager.Object);

        var (token, expiresAtUtc) = await service.CreateTokenAsync(user, CancellationToken.None);
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;

        Assert.Contains(claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.True(expiresAtUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ValidateOptions_RejectsShortSigningKeys()
    {
        var options = new JwtOptions
        {
            Key = "short",
            Issuer = "DoctorPracticeCmsApi.Tests",
            Audience = "DoctorPracticeCmsApi.Tests"
        };

        Assert.Throws<InvalidOperationException>(() => JwtTokenService.ValidateOptions(options));
    }

    private static Mock<UserManager<AppUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<AppUser>>>());
    }
}
