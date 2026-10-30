using DoctorPracticeCmsApi.Contracts;
using DoctorPracticeCmsApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorPracticeCmsApi.Services;

public sealed class PracticeSiteService(PracticeDbContext dbContext) : IPracticeSiteService
{
    public async Task<PublicPracticeSiteResponse> GetPublicSiteAsync(CancellationToken cancellationToken)
    {
        var doctors = await dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(profile => profile.IsPublished)
            .OrderBy(profile => profile.Id)
            .Select(profile => new DoctorProfileResponse(
                profile.Id,
                profile.FullName,
                profile.Credentials,
                profile.Specialty,
                profile.Biography,
                profile.PhotoUrl,
                profile.PracticePhone,
                profile.PracticeEmail,
                profile.IsPublished))
            .ToListAsync(cancellationToken);

        var pages = await dbContext.ContentPages
            .AsNoTracking()
            .Where(page => page.IsPublished)
            .OrderBy(page => page.Title)
            .Select(page => new ContentPageResponse(
                page.Id,
                page.Title,
                page.Slug,
                page.Summary,
                page.Body,
                page.IsPublished,
                page.CreatedAtUtc,
                page.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        var services = await dbContext.PracticeServices
            .AsNoTracking()
            .Where(service => service.IsPublished)
            .OrderBy(service => service.SortOrder)
            .ThenBy(service => service.Name)
            .Select(service => new PracticeServiceResponse(
                service.Id,
                service.Name,
                service.Slug,
                service.Description,
                service.DurationMinutes,
                service.IsPublished,
                service.SortOrder))
            .ToListAsync(cancellationToken);

        var faqs = await dbContext.Faqs
            .AsNoTracking()
            .Where(faq => faq.IsPublished)
            .OrderBy(faq => faq.SortOrder)
            .ThenBy(faq => faq.Id)
            .Select(faq => new FaqResponse(
                faq.Id,
                faq.Question,
                faq.Answer,
                faq.IsPublished,
                faq.SortOrder))
            .ToListAsync(cancellationToken);

        return new PublicPracticeSiteResponse(doctors, pages, services, faqs);
    }
}
