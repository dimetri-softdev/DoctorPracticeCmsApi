using System.ComponentModel.DataAnnotations;
using DoctorPracticeCmsApi.Models;

namespace DoctorPracticeCmsApi.Contracts;

/// <summary>Fields used to create or update a content page.</summary>
public sealed record ContentPageRequest
{
    /// <summary>Page title shown to visitors.</summary>
    [Required, StringLength(160)]
    public required string Title { get; init; }

    /// <summary>Unique URL-friendly page path.</summary>
    [Required, RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Use lowercase letters, numbers, and hyphens.")]
    [StringLength(180)]
    public required string Slug { get; init; }

    /// <summary>Short page summary.</summary>
    [StringLength(500)]
    public string Summary { get; init; } = string.Empty;

    /// <summary>Page content managed by the practice.</summary>
    [Required]
    public required string Body { get; init; }

    /// <summary>Whether the page is publicly visible.</summary>
    public bool IsPublished { get; init; }
}

/// <summary>Published content page details.</summary>
public sealed record ContentPageResponse(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string Body,
    bool IsPublished,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Fields used to create or update an appointment service.</summary>
public sealed record PracticeServiceRequest
{
    /// <summary>Service name shown to visitors.</summary>
    [Required, StringLength(160)]
    public required string Name { get; init; }

    /// <summary>Unique URL-friendly service path.</summary>
    [Required, RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Use lowercase letters, numbers, and hyphens.")]
    [StringLength(180)]
    public required string Slug { get; init; }

    /// <summary>Public description of the service.</summary>
    [Required, StringLength(4000)]
    public required string Description { get; init; }

    /// <summary>Typical appointment duration in minutes.</summary>
    [Range(5, 480)]
    public int DurationMinutes { get; init; }

    /// <summary>Whether the service is publicly visible.</summary>
    public bool IsPublished { get; init; }

    /// <summary>Display order in the public service list.</summary>
    public int SortOrder { get; init; }
}

/// <summary>Appointment service details shown on the public website.</summary>
public sealed record PracticeServiceResponse(
    int Id,
    string Name,
    string Slug,
    string Description,
    int DurationMinutes,
    bool IsPublished,
    int SortOrder);

/// <summary>Fields used to create or update a frequently asked question.</summary>
public sealed record FaqRequest
{
    /// <summary>Question displayed to visitors.</summary>
    [Required, StringLength(500)]
    public required string Question { get; init; }

    /// <summary>Public answer to the question.</summary>
    [Required, StringLength(4000)]
    public required string Answer { get; init; }

    /// <summary>Whether the FAQ is publicly visible.</summary>
    public bool IsPublished { get; init; }

    /// <summary>Display order in the public FAQ list.</summary>
    public int SortOrder { get; init; }
}

/// <summary>A frequently asked question and its public answer.</summary>
public sealed record FaqResponse(int Id, string Question, string Answer, bool IsPublished, int SortOrder);

/// <summary>Fields used to create or update a practice doctor profile.</summary>
public sealed record DoctorProfileRequest
{
    /// <summary>Doctor's name.</summary>
    [Required, StringLength(160)]
    public required string FullName { get; init; }

    /// <summary>Professional qualifications.</summary>
    [StringLength(200)]
    public string Credentials { get; init; } = string.Empty;

    /// <summary>Doctor's specialty.</summary>
    [StringLength(160)]
    public string Specialty { get; init; } = string.Empty;

    /// <summary>Biography intended for publication.</summary>
    [StringLength(4000)]
    public string Biography { get; init; } = string.Empty;

    /// <summary>Optional public profile photograph URL.</summary>
    [Url, StringLength(1000)]
    public string? PhotoUrl { get; init; }

    /// <summary>Public practice phone number.</summary>
    [Phone, StringLength(32)]
    public string? PracticePhone { get; init; }

    /// <summary>Public practice email address.</summary>
    [EmailAddress, StringLength(256)]
    public string? PracticeEmail { get; init; }

    /// <summary>Whether the profile is publicly visible.</summary>
    public bool IsPublished { get; init; }
}

/// <summary>Doctor profile details.</summary>
public sealed record DoctorProfileResponse(
    int Id,
    string FullName,
    string Credentials,
    string Specialty,
    string Biography,
    string? PhotoUrl,
    string? PracticePhone,
    string? PracticeEmail,
    bool IsPublished);

/// <summary>Public information needed to request an appointment.</summary>
public sealed record CreateAppointmentRequest
{
    /// <summary>Patient name used to contact them about the request.</summary>
    [Required, StringLength(120)]
    public required string PatientName { get; init; }

    /// <summary>Patient email address.</summary>
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    /// <summary>Patient phone number.</summary>
    [Required, Phone, StringLength(32)]
    public required string PhoneNumber { get; init; }

    /// <summary>Requested appointment time, including its time-zone offset.</summary>
    public DateTimeOffset RequestedAt { get; init; }
}

/// <summary>Appointment request details visible to authorized practice staff.</summary>
public sealed record AppointmentResponse(
    int Id,
    string PatientName,
    string Email,
    string PhoneNumber,
    DateTimeOffset RequestedAt,
    DateTimeOffset SubmittedAtUtc,
    AppointmentStatus Status);

/// <summary>Administrative update for an appointment request status.</summary>
public sealed record UpdateAppointmentStatusRequest
{
    /// <summary>New appointment status.</summary>
    [EnumDataType(typeof(AppointmentStatus))]
    public AppointmentStatus Status { get; init; }
}

/// <summary>Credentials used by practice staff to sign in.</summary>
public sealed record LoginRequest
{
    /// <summary>Administrator email address.</summary>
    [Required, EmailAddress]
    public required string Email { get; init; }

    /// <summary>Administrator password.</summary>
    [Required]
    public required string Password { get; init; }
}

/// <summary>Bearer token returned after successful staff sign-in.</summary>
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);

/// <summary>Publicly visible content used by a practice website.</summary>
public sealed record PublicPracticeSiteResponse(
    IReadOnlyList<DoctorProfileResponse> Doctors,
    IReadOnlyList<ContentPageResponse> Pages,
    IReadOnlyList<PracticeServiceResponse> Services,
    IReadOnlyList<FaqResponse> Faqs);
