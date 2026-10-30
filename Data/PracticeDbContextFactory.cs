using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DoctorPracticeCmsApi.Data;

public sealed class PracticeDbContextFactory : IDesignTimeDbContextFactory<PracticeDbContext>
{
    public PracticeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PracticeDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DoctorPracticeCmsApi;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new PracticeDbContext(options);
    }
}
