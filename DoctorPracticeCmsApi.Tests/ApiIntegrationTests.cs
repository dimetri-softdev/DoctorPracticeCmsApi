using System.Net;
using System.Net.Http.Json;
using DoctorPracticeCmsApi.Models;

namespace DoctorPracticeCmsApi.Tests;

public sealed class ApiIntegrationTests(DoctorPracticeCmsApiFactory factory)
    : IClassFixture<DoctorPracticeCmsApiFactory>
{
    [Fact]
    public async Task GetPublicSite_ReturnsPublishedContentOnly()
    {
        await factory.InitializeDatabaseAsync(dbContext =>
        {
            dbContext.ContentPages.AddRange(
                new ContentPage
                {
                    Title = "About",
                    Slug = "about",
                    Body = "About the practice.",
                    IsPublished = true
                },
                new ContentPage
                {
                    Title = "Draft",
                    Slug = "draft",
                    Body = "Internal draft.",
                    IsPublished = false
                });
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/site");
        var site = await response.Content.ReadFromJsonAsync<PublicSiteTestResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(site);
        Assert.Contains(site.Pages, page => page.Slug == "about");
        Assert.DoesNotContain(site.Pages, page => page.Slug == "draft");
    }

    [Fact]
    public async Task CreateAppointmentRequest_ReturnsCreated()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            patientName = "Taylor Example",
            email = "taylor@example.com",
            phoneNumber = "+27123456789",
            requestedAt = DateTimeOffset.UtcNow.AddDays(3)
        };

        var response = await client.PostAsJsonAsync("/api/appointments", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AppointmentRequests_AreNotPubliclyReadable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/appointments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiDocument_IsAvailable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("appointments", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAppointmentRequest_WithPastTime_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            patientName = "Taylor Example",
            email = "taylor@example.com",
            phoneNumber = "+27123456789",
            requestedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var response = await client.PostAsJsonAsync("/api/appointments", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record PublicSiteTestResponse(IReadOnlyList<PageTestResponse> Pages);

    private sealed record PageTestResponse(string Slug);
}
