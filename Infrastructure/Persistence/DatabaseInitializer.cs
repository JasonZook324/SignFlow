using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace SignFlow.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var connString = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connString)) return;

        try
        {
            // Ensure database exists (PostgreSQL): connect to 'postgres' and create target DB if missing
            var csb = new NpgsqlConnectionStringBuilder(connString);
            var targetDb = csb.Database;
            csb.Database = "postgres"; // admin database
            await using (var conn = new NpgsqlConnection(csb.ToString()))
            {
                await conn.OpenAsync();
                await using var checkCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", conn);
                checkCmd.Parameters.AddWithValue("name", targetDb);
                var exists = await checkCmd.ExecuteScalarAsync() is not null;
                if (!exists)
                {
                    await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{targetDb}\"", conn);
                    await createCmd.ExecuteNonQueryAsync();
                }
            }

            // Apply migrations
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }
        catch
        {
            // Swallow in dev; surfacing at app start can block. Logs can be added later.
            // In production, this should be logged and not swallowed.
        }
    }
}
