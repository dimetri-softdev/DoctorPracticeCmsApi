using DoctorPracticeCmsApi.Contracts;

namespace DoctorPracticeCmsApi.Services;

public interface IPracticeSiteService
{
    Task<PublicPracticeSiteResponse> GetPublicSiteAsync(CancellationToken cancellationToken);
}
