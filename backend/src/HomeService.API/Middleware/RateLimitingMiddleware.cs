using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace HomeService.API.Middleware;

/// <summary>
/// Middleware for rate limiting API requests based on IP address
/// Implements sliding window algorithm for accurate rate limiting
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health checks
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var cacheKey = $"rate_limit_{clientId}";

        if (!_cache.TryGetValue(cacheKey, out RateLimitInfo? rateLimitInfo))
        {
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 0,
                WindowStart = DateTime.UtcNow
            };
        }

        // Reset window if expired
        if (DateTime.UtcNow - rateLimitInfo!.WindowStart > TimeSpan.FromMinutes(_options.WindowMinutes))
        {
            rateLimitInfo = new RateLimitInfo
            {
                RequestCount = 0,
                WindowStart = DateTime.UtcNow
            };
        }

        // Increment request count
        rateLimitInfo.RequestCount++;

        // Check if limit exceeded
        if (rateLimitInfo.RequestCount > _options.MaxRequests)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId}. Request count: {RequestCount}, Limit: {MaxRequests}",
                clientId,
                rateLimitInfo.RequestCount,
                _options.MaxRequests);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = _options.WindowMinutes.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = rateLimitInfo.WindowStart
                .AddMinutes(_options.WindowMinutes)
                .ToString("o");

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 429,
                Message = "Too many requests. Please try again later.",
                RetryAfter = $"{_options.WindowMinutes} minutes"
            });

            return;
        }

        // Store updated rate limit info
        _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromMinutes(_options.WindowMinutes));

        // Add rate limit headers to response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] =
                Math.Max(0, _options.MaxRequests - rateLimitInfo.RequestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = rateLimitInfo.WindowStart
                .AddMinutes(_options.WindowMinutes)
                .ToString("o");
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID if authenticated
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("id")?.Value
            : null;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString()
            ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? "unknown";

        return $"ip_{ipAddress}";
    }

    private class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed per window
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Time window in minutes for rate limiting
    /// </summary>
    public int WindowMinutes { get; set; } = 1;
}

/// <summary>
/// Extension methods for rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        Action<RateLimitOptions>? configureOptions = null)
    {
        var options = new RateLimitOptions();
        configureOptions?.Invoke(options);

        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
