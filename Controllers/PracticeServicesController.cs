using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Appointment services offered by the medical practice.</summary>
[ApiController]
[Route("api/services")]
public sealed class PracticeServicesController(PracticeDbContext dbContext) : ControllerBase
{
    /// <summary>Lists published services.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<PracticeServiceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PracticeServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var services = await dbContext.PracticeServices
            .AsNoTracking()
            .Where(item => item.IsPublished || includeDrafts)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(services);
    }

    /// <summary>Gets a service by URL slug.</summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<PracticeServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PracticeServiceResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var service = await dbContext.PracticeServices
            .AsNoTracking()
            .Where(item => item.Slug == slug && (item.IsPublished || includeDrafts))
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return service is null ? NotFound() : Ok(service);
    }

    /// <summary>Creates a service.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<PracticeServiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PracticeServiceResponse>> Create(
        PracticeServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (await dbContext.PracticeServices.AnyAsync(item => item.Slug == request.Slug, cancellationToken))
        {
            return SlugConflict();
        }

        var service = new PracticeService
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder
        };
        dbContext.PracticeServices.Add(service);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetBySlug), new { slug = service.Slug }, ToResponse(service));
    }

    /// <summary>Updates a service.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        PracticeServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await dbContext.PracticeServices.FindAsync([id], cancellationToken);
        if (service is null)
        {
            return NotFound();
        }

        if (await dbContext.PracticeServices.AnyAsync(item => item.Id != id && item.Slug == request.Slug, cancellationToken))
        {
            return SlugConflict();
        }

        service.Name = request.Name;
        service.Slug = request.Slug;
        service.Description = request.Description;
        service.DurationMinutes = request.DurationMinutes;
        service.IsPublished = request.IsPublished;
        service.SortOrder = request.SortOrder;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a service.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var service = await dbContext.PracticeServices.FindAsync([id], cancellationToken);
        if (service is null)
        {
            return NotFound();
        }

        dbContext.PracticeServices.Remove(service);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private ConflictObjectResult SlugConflict() =>
        Conflict(new ProblemDetails { Title = "Service slug already exists." });

    private static PracticeServiceResponse ToResponse(PracticeService service) =>
        new(service.Id, service.Name, service.Slug, service.Description,
            service.DurationMinutes, service.IsPublished, service.SortOrder);
}
