using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Doctor profiles published on the practice website.</summary>
[ApiController]
[Route("api/doctors")]
public sealed class DoctorProfilesController(PracticeDbContext dbContext) : ControllerBase
{
    /// <summary>Lists published doctor profiles.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<DoctorProfileResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorProfileResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var doctors = await dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(item => item.IsPublished || includeDrafts)
            .OrderBy(item => item.FullName)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(doctors);
    }

    /// <summary>Gets a doctor profile by identifier.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<DoctorProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorProfileResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var includeDrafts = User.IsInRole("Admin");
        var doctor = await dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(item => item.Id == id && (item.IsPublished || includeDrafts))
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return doctor is null ? NotFound() : Ok(doctor);
    }

    /// <summary>Creates a doctor profile.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<DoctorProfileResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<DoctorProfileResponse>> Create(
        DoctorProfileRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = new DoctorProfile();
        Apply(request, doctor);
        dbContext.DoctorProfiles.Add(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, ToResponse(doctor));
    }

    /// <summary>Updates a doctor profile.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        DoctorProfileRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await dbContext.DoctorProfiles.FindAsync([id], cancellationToken);
        if (doctor is null)
        {
            return NotFound();
        }

        Apply(request, doctor);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a doctor profile.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var doctor = await dbContext.DoctorProfiles.FindAsync([id], cancellationToken);
        if (doctor is null)
        {
            return NotFound();
        }

        dbContext.DoctorProfiles.Remove(doctor);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static void Apply(DoctorProfileRequest request, DoctorProfile doctor)
    {
        doctor.FullName = request.FullName;
        doctor.Credentials = request.Credentials;
        doctor.Specialty = request.Specialty;
        doctor.Biography = request.Biography;
        doctor.PhotoUrl = request.PhotoUrl;
        doctor.PracticePhone = request.PracticePhone;
        doctor.PracticeEmail = request.PracticeEmail;
        doctor.IsPublished = request.IsPublished;
    }

    private static DoctorProfileResponse ToResponse(DoctorProfile doctor) =>
        new(doctor.Id, doctor.FullName, doctor.Credentials, doctor.Specialty,
            doctor.Biography, doctor.PhotoUrl, doctor.PracticePhone,
            doctor.PracticeEmail, doctor.IsPublished);
}
