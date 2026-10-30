using System.ComponentModel.DataAnnotations;

namespace DoctorPracticeCmsApi.Models;

public sealed class AppointmentRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTimeOffset RequestedAtUtc { get; set; }

    public DateTimeOffset SubmittedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Declined,
    Completed
}
