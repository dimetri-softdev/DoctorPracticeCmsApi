using System.ComponentModel.DataAnnotations;

namespace DoctorPracticeCmsApi.Models;

public sealed class PracticeService
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Range(5, 480)]
    public int DurationMinutes { get; set; }

    public bool IsPublished { get; set; }

    public int SortOrder { get; set; }
}
