using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.Data;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var environment = services.GetRequiredService<IHostEnvironment>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Apply pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed data only in Development or Staging environments
            if (environment.IsDevelopment() || environment.IsStaging())
            {
                logger.LogInformation("Seeding database for {Environment} environment...", environment.EnvironmentName);
                await DbInitializer.SeedAsync(context);
                logger.LogInformation("Database seeding completed successfully.");
            }
            else
            {
                logger.LogInformation("Skipping database seeding for {Environment} environment.", environment.EnvironmentName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
