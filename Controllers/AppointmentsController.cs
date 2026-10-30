using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Controllers;

/// <summary>Appointment request intake and staff scheduling endpoints.</summary>
[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(PracticeDbContext dbContext) : ControllerBase
{
    /// <summary>Submits an appointment request for the practice to review.</summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("public-requests")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AppointmentResponse>> Create(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (request.RequestedAt <= now || request.RequestedAt > now.AddMonths(12))
        {
            ModelState.AddModelError(
                nameof(request.RequestedAt),
                "Choose a future appointment time within the next 12 months.");
            return ValidationProblem(ModelState);
        }

        var appointment = new AppointmentRequest
        {
            PatientName = request.PatientName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            RequestedAtUtc = request.RequestedAt.ToUniversalTime(),
            SubmittedAtUtc = now
        };
        dbContext.AppointmentRequests.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = appointment.Id },
            ToResponse(appointment));
    }

    /// <summary>Lists appointment requests for authorized practice staff.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<AppointmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var appointments = await dbContext.AppointmentRequests
            .AsNoTracking()
            .OrderBy(item => item.RequestedAtUtc)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(appointments);
    }

    /// <summary>Gets one appointment request for authorized practice staff.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<AppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var appointment = await dbContext.AppointmentRequests
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => ToResponse(item))
            .SingleOrDefaultAsync(cancellationToken);

        return appointment is null ? NotFound() : Ok(appointment);
    }

    /// <summary>Updates the review status of an appointment request.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            ModelState.AddModelError(nameof(request.Status), "Choose a valid appointment status.");
            return ValidationProblem(ModelState);
        }

        var appointment = await dbContext.AppointmentRequests.FindAsync([id], cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        appointment.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static AppointmentResponse ToResponse(AppointmentRequest appointment) =>
        new(appointment.Id, appointment.PatientName, appointment.Email,
            appointment.PhoneNumber, appointment.RequestedAtUtc,
            appointment.SubmittedAtUtc, appointment.Status);
}
