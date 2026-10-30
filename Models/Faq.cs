using System.ComponentModel.DataAnnotations;

namespace DoctorPracticeCmsApi.Models;

public sealed class Faq
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Answer { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public int SortOrder { get; set; }
}
