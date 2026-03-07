using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace MediaBrowser.Media;

/// <summary>
/// Integration tests to test using different SQL DBs.
/// </summary>
public class DbTests
{
    [Test(Description = "The backend should successfully boot and update the schema on boot for each DB type."),
     TestCase(DbType.MySql),
     TestCase(DbType.Postgres),
     TestCase(DbType.Sqlite),
     TestCase(DbType.SqlServer)]
    public async Task Test(DbType dbType)
    {
        await using var factory = new MediaBrowserWebApplicationFactory(dbType);

        await factory.StartServerAsync();

        using var scope = factory.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var appliedContext = await db.Database.GetAppliedMigrationsAsync();
        appliedContext.ShouldNotBeEmpty("The migrations should be applied successfully on boot, indicating that the backend can connect to the database and update the schema as needed.");
        db.Database.ProviderName.ShouldBe(dbType switch
        {
            DbType.MySql => "Pomelo.EntityFrameworkCore.MySql",
            DbType.Postgres => "Npgsql.EntityFrameworkCore.PostgreSQL",
            DbType.SqlServer => "Microsoft.EntityFrameworkCore.SqlServer",
            _ => "Microsoft.EntityFrameworkCore.Sqlite"
        }, "The expected database provider should be used based on the configuration.");
    }
}