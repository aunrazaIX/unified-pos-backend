using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UnifiedPOS.Infrastructure.Databases;

public sealed class POSContextFactory : IDesignTimeDbContextFactory<POSContext>
{
    public POSContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__POSConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=UnifiedPOS_DesignTime;Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<POSContext>().UseSqlServer(connectionString).Options;
        return new POSContext(options);
    }
}
