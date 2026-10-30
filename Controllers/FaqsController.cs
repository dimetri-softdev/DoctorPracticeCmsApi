using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Frequently asked questions managed by the practice.</summary>
[ApiController]
[Route("api/faqs")]
public sealed class FaqsController(PracticeDbContext dbContext) : ControllerBase
{
    /// <summary>Lists published frequently asked questions.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<FaqResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FaqResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var faqs = await dbContext.Faqs
            .AsNoTracking()
            .Where(item => item.IsPublished || includeDrafts)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(faqs);
    }

    /// <summary>Gets one frequently asked question by identifier.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<FaqResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FaqResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var faq = await dbContext.Faqs
            .AsNoTracking()
            .Where(item => item.Id == id && (item.IsPublished || includeDrafts))
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return faq is null ? NotFound() : Ok(faq);
    }

    /// <summary>Creates a frequently asked question.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<FaqResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<FaqResponse>> Create(FaqRequest request, CancellationToken cancellationToken)
    {
        var faq = new Faq
        {
            Question = request.Question,
            Answer = request.Answer,
            IsPublished = request.IsPublished,
            SortOrder = request.SortOrder
        };
        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = faq.Id }, ToResponse(faq));
    }

    /// <summary>Updates a frequently asked question.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, FaqRequest request, CancellationToken cancellationToken)
    {
        var faq = await dbContext.Faqs.FindAsync([id], cancellationToken);
        if (faq is null)
        {
            return NotFound();
        }

        faq.Question = request.Question;
        faq.Answer = request.Answer;
        faq.IsPublished = request.IsPublished;
        faq.SortOrder = request.SortOrder;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a frequently asked question.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var faq = await dbContext.Faqs.FindAsync([id], cancellationToken);
        if (faq is null)
        {
            return NotFound();
        }

        dbContext.Faqs.Remove(faq);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static FaqResponse ToResponse(Faq faq) =>
        new(faq.Id, faq.Question, faq.Answer, faq.IsPublished, faq.SortOrder);
}
