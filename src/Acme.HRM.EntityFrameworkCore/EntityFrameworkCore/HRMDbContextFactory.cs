using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Acme.HRM.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class HRMDbContextFactory : IDesignTimeDbContextFactory<HRMDbContext>
{
    public HRMDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        //var configuration = BuildConfiguration();

        //HRMEfCoreEntityExtensionMappings.Configure();

        //var builder = new DbContextOptionsBuilder<HRMDbContext>()
        //    .UseNpgsql(configuration.GetConnectionString("Default"));

        var builder = new DbContextOptionsBuilder<HRMDbContext>();

        builder.UseNpgsql(
            "Host=localhost;Port=5432;Database=HRM;User ID=postgres;Password=1234567"
        );

        return new HRMDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Acme.HRM.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
