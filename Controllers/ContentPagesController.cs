using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Website pages managed by the practice.</summary>
[ApiController]
[Route("api/pages")]
public sealed class ContentPagesController(PracticeDbContext dbContext) : ControllerBase
{
    /// <summary>Lists published website pages.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<ContentPageResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContentPageResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var pages = await dbContext.ContentPages
            .AsNoTracking()
            .Where(page => page.IsPublished || includeDrafts)
            .OrderBy(page => page.Title)
            .Select(page => ToResponse(page))
            .ToListAsync(cancellationToken);

        return Ok(pages);
    }

    /// <summary>Gets a page by its URL slug.</summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ContentPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentPageResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var page = await dbContext.ContentPages
            .AsNoTracking()
            .Where(item => item.Slug == slug && (item.IsPublished || includeDrafts))
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return page is null ? NotFound() : Ok(page);
    }

    /// <summary>Creates a page draft or published page.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ContentPageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContentPageResponse>> Create(
        ContentPageRequest request,
        CancellationToken cancellationToken)
    {
        if (await dbContext.ContentPages.AnyAsync(page => page.Slug == request.Slug, cancellationToken))
        {
            return SlugConflict();
        }

        var page = new ContentPage
        {
            Title = request.Title,
            Slug = request.Slug,
            Summary = request.Summary,
            Body = request.Body,
            IsPublished = request.IsPublished
        };
        dbContext.ContentPages.Add(page);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetBySlug), new { slug = page.Slug }, ToResponse(page));
    }

    /// <summary>Replaces all editable fields of a page.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        ContentPageRequest request,
        CancellationToken cancellationToken)
    {
        var page = await dbContext.ContentPages.FindAsync([id], cancellationToken);
        if (page is null)
        {
            return NotFound();
        }

        if (await dbContext.ContentPages.AnyAsync(item => item.Id != id && item.Slug == request.Slug, cancellationToken))
        {
            return SlugConflict();
        }

        page.Title = request.Title;
        page.Slug = request.Slug;
        page.Summary = request.Summary;
        page.Body = request.Body;
        page.IsPublished = request.IsPublished;
        page.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>Deletes a website page.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var page = await dbContext.ContentPages.FindAsync([id], cancellationToken);
        if (page is null)
        {
            return NotFound();
        }

        dbContext.ContentPages.Remove(page);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private ConflictObjectResult SlugConflict() =>
        Conflict(new ProblemDetails { Title = "Page slug already exists." });

    private static ContentPageResponse ToResponse(ContentPage page) =>
        new(page.Id, page.Title, page.Slug, page.Summary, page.Body,
            page.IsPublished, page.CreatedAtUtc, page.UpdatedAtUtc);
}
