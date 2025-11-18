using Hangfire;
using Hangfire.SqlServer;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Infrastructure.AI.Services;
using HomeService.Infrastructure.Data;
using HomeService.Infrastructure.Identity;
using HomeService.Infrastructure.Repositories;
using HomeService.Infrastructure.Services;
using HomeService.Infrastructure.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Identity & Authentication
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // Payment Services
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentGatewayService, StripePaymentService>();

        // Communication Services
        services.AddScoped<IOtpService, OtpService>();

        // Notification Services
        services.AddScoped<ISmsService, Services.Notifications.TwilioSmsService>();
        services.AddScoped<IEmailService, Services.Notifications.SendGridEmailService>();
        services.AddSingleton<IPushNotificationService, Services.Notifications.FirebasePushNotificationService>();
        services.AddScoped<INotificationService, Services.Notifications.NotificationService>();

        // File Storage Service
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        // AI Services (Semantic Kernel)
        services.AddSingleton<SemanticKernelService>();
        services.AddScoped<ChatbotService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<SentimentAnalysisService>();
        services.AddScoped<SemanticSearchService>();

        // Redis Cache (or in-memory fallback)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Note: AddStackExchangeRedisCache requires Microsoft.Extensions.Caching.StackExchangeRedis package
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = redisConnection;
            // });
            
            // Use in-memory cache as fallback since Redis package is not installed
            services.AddDistributedMemoryCache();
        }
        else
        {
            // Use in-memory distributed cache when Redis is not configured
            services.AddDistributedMemoryCache();
        }

        // Hangfire Background Jobs (optional - requires SQL Server)
        var enableHangfire = configuration.GetValue<bool>("Hangfire:Enabled", false);
        if (enableHangfire && !string.IsNullOrEmpty(connectionString))
        {
            try
            {
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        SchemaName = "hangfire"
                    }));

                services.AddHangfireServer(options =>
                {
                    options.WorkerCount = Environment.ProcessorCount * 2;
                    options.Queues = new[] { "default", "critical", "normal", "low" };
                });
            }
            catch (Exception)
            {
                // Hangfire configuration failed - app will run without background jobs
                Console.WriteLine("Warning: Hangfire could not be initialized. Background jobs will be disabled.");
            }
        }
        else
        {
            // Hangfire is disabled - use in-memory job queue for development
            Console.WriteLine("Info: Hangfire is disabled. Background jobs will not be processed.");
        }

        return services;
    }
}
