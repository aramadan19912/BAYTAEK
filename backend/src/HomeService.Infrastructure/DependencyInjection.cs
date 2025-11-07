using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Infrastructure.AI.Services;
using HomeService.Infrastructure.Data;
using HomeService.Infrastructure.Identity;
using HomeService.Infrastructure.Repositories;
using HomeService.Infrastructure.Services;
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

        // Payment Services
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentGatewayService, StripePaymentService>();

        // Communication Services
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, SendGridEmailService>();

        // AI Services (Semantic Kernel)
        services.AddSingleton<SemanticKernelService>();
        services.AddScoped<ChatbotService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<SentimentAnalysisService>();
        services.AddScoped<SemanticSearchService>();

        // Redis Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }

        return services;
    }
}
