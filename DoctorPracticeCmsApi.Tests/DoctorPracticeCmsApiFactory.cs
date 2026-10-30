using DoctorPracticeCmsApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DoctorPracticeCmsApi.Tests;

public sealed class DoctorPracticeCmsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"DoctorPracticeCmsTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<PracticeDbContext>>();
            services.RemoveAll<DbContextOptions<PracticeDbContext>>();
            services.RemoveAll<PracticeDbContext>();
            services.AddDbContext<PracticeDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task InitializeDatabaseAsync(Action<PracticeDbContext>? seed = null)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        seed?.Invoke(dbContext);
        await dbContext.SaveChangesAsync();
    }
}
