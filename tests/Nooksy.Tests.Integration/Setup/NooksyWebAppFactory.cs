using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nooksy.DataAccess.Data;
using Nooksy.Utility;

namespace Nooksy.Tests.Integration.Setup;

public class NooksyWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Replace with InMemory database
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("NooksyTestDb_" + Guid.NewGuid()));

            // Configure Stripe with test key
            services.Configure<StripeSetting>(opts =>
            {
                opts.SecretKey = "sk_test_fake";
                opts.PublishableKey = "pk_test_fake";
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Seed test data
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            DatabaseSeeder.Seed(db);
        });
    }
}
