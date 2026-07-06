using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TRC.Infrastructure.Persistence;

// Used only by `dotnet ef` at design time. Reads the connection string from the
// TRC_MIGRATIONS_CONNECTION env var, or a local default. Use the Supabase
// direct/session connection (port 5432) for migrations (SRS §2.4).
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("TRC_MIGRATIONS_CONNECTION")
                   ?? "Host=localhost;Database=trc;Username=postgres;Password=postgres;Port=5432";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;
        return new AppDbContext(options);
    }
}
