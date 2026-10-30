using DoctorPracticeCmsApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Data;

public sealed class PracticeDbContext(DbContextOptions<PracticeDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole, string>(options)
{
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();

    public DbSet<PracticeService> PracticeServices => Set<PracticeService>();

    public DbSet<Faq> Faqs => Set<Faq>();

    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();

    public DbSet<AppointmentRequest> AppointmentRequests => Set<AppointmentRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContentPage>()
            .HasIndex(page => page.Slug)
            .IsUnique();

        modelBuilder.Entity<PracticeService>()
            .HasIndex(service => service.Slug)
            .IsUnique();

        modelBuilder.Entity<DoctorProfile>()
            .HasIndex(profile => profile.IsPublished);
    }
}
