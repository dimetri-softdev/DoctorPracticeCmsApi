using System.ComponentModel.DataAnnotations;

namespace DoctorPracticeCmsApi.Models;

public sealed class DoctorProfile
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string Credentials { get; set; } = string.Empty;

    [StringLength(160)]
    public string Specialty { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Biography { get; set; } = string.Empty;

    [Url]
    [StringLength(1000)]
    public string? PhotoUrl { get; set; }

    [Phone]
    [StringLength(32)]
    public string? PracticePhone { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string? PracticeEmail { get; set; }

    public bool IsPublished { get; set; }
}
