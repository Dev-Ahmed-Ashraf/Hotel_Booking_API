using Hotel_Booking_API.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Data.Sqlite;

public class TestWebApplicationFactory : WebApplicationFactory<Hotel_Booking_API.Program>
{
    static TestWebApplicationFactory()
    {
        SQLitePCL.Batteries_V2.Init();
    }

    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // remove existing db context configuration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // create sqlite in-memory connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // build database schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
