using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DoctorPracticeCmsApi.Models;

public sealed class AppUser : IdentityUser
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;
}
