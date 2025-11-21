using Hotel_Booking_API.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TestWebApplicationFactory : WebApplicationFactory<Hotel_Booking_API.Program>
{
    private readonly string _dbFileName = $"test_db_{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set up the configuration to use test settings
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbFileName}",
                ["Jwt:Key"] = "TestKeyForTestingPurposesOnly",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the app's ApplicationDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add SQLite in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_dbFileName}");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory>>();

                // Ensure the database is created
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data if needed
                    // await SeedData.InitializeAsync(db);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
                }
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        // Clean up the test database
        if (File.Exists(_dbFileName))
        {
            try
            {
                File.Delete(_dbFileName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        base.Dispose(disposing);
    }
}
