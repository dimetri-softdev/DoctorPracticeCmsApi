using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Models;
using DoctorPracticeCmsApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Practice staff authentication endpoints.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<AppUser> userManager,
    IJwtTokenService tokenService) : ControllerBase
{
    /// <summary>Authenticates a practice administrator and returns a JWT.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("public-requests")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return InvalidCredentials();
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return InvalidCredentials();
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            return InvalidCredentials();
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var (token, expiresAtUtc) = await tokenService.CreateTokenAsync(user, cancellationToken);
        return Ok(new TokenResponse(token, expiresAtUtc));
    }

    private UnauthorizedObjectResult InvalidCredentials() =>
        Unauthorized(new ProblemDetails
        {
            Title = "Invalid credentials",
            Detail = "The email or password is incorrect."
        });
}
