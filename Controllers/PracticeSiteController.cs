using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Public website content for the medical practice.</summary>
[ApiController]
[Route("api/site")]
public sealed class PracticeSiteController(IPracticeSiteService siteService) : ControllerBase
{
    /// <summary>Returns all currently published practice website content.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PublicPracticeSiteResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicPracticeSiteResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await siteService.GetPublicSiteAsync(cancellationToken));
}
