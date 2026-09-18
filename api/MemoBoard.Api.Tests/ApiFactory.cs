using MemoBoard.Api;
using MemoBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MemoBoard.Api.Tests
{
    // The real application on a SQLite database in memory, with a signing
    // key of its own and, unless a test says otherwise, a login limit high
    // enough never to get in the way.
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public int LoginAttemptsPerMinute { get; init; } = 1000;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();
            builder.UseSetting("Jwt:Key", "test-only-key-that-is-long-enough-for-hmac-sha256-0123456789");
            builder.UseSetting("RateLimiting:LoginAttemptsPerMinute", LoginAttemptsPerMinute.ToString());
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<MemoContext>));
                services.Remove(descriptor);
                services.AddDbContext<MemoContext>(options => options.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _connection.Dispose();
        }
    }
}
