namespace HomeService.API.Middleware;

/// <summary>
/// Middleware for adding security headers to HTTP responses
/// Implements OWASP security best practices
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger,
        SecurityHeadersOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing request
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // X-Content-Type-Options: Prevents MIME-sniffing
            if (!headers.ContainsKey("X-Content-Type-Options"))
            {
                headers.Add("X-Content-Type-Options", "nosniff");
            }

            // X-Frame-Options: Prevents clickjacking attacks
            if (!headers.ContainsKey("X-Frame-Options") && _options.EnableFrameOptions)
            {
                headers.Add("X-Frame-Options", _options.FrameOptions);
            }

            // X-XSS-Protection: Enables XSS filter in older browsers
            if (!headers.ContainsKey("X-XSS-Protection") && _options.EnableXssProtection)
            {
                headers.Add("X-XSS-Protection", "1; mode=block");
            }

            // Referrer-Policy: Controls referrer information
            if (!headers.ContainsKey("Referrer-Policy") && _options.EnableReferrerPolicy)
            {
                headers.Add("Referrer-Policy", _options.ReferrerPolicy);
            }

            // Content-Security-Policy: Prevents XSS, clickjacking, and other code injection attacks
            if (!headers.ContainsKey("Content-Security-Policy") && _options.EnableContentSecurityPolicy)
            {
                headers.Add("Content-Security-Policy", _options.ContentSecurityPolicy);
            }

            // Strict-Transport-Security: Enforces HTTPS
            if (!headers.ContainsKey("Strict-Transport-Security") && _options.EnableHsts)
            {
                headers.Add("Strict-Transport-Security", $"max-age={_options.HstsMaxAge}; includeSubDomains");
            }

            // Permissions-Policy: Controls browser features and APIs
            if (!headers.ContainsKey("Permissions-Policy") && _options.EnablePermissionsPolicy)
            {
                headers.Add("Permissions-Policy", _options.PermissionsPolicy);
            }

            // Remove potentially dangerous headers
            headers.Remove("X-Powered-By");
            headers.Remove("Server");

            // Add custom security headers
            if (!headers.ContainsKey("X-Content-Security-Policy"))
            {
                headers.Add("X-Content-Security-Policy", "default-src 'self'");
            }

            _logger.LogDebug("Security headers added to response for {Path}", context.Request.Path);

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Configuration options for security headers
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Enable X-Frame-Options header (default: true)
    /// </summary>
    public bool EnableFrameOptions { get; set; } = true;

    /// <summary>
    /// X-Frame-Options value (default: DENY)
    /// Options: DENY, SAMEORIGIN, ALLOW-FROM uri
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Enable X-XSS-Protection header (default: true)
    /// </summary>
    public bool EnableXssProtection { get; set; } = true;

    /// <summary>
    /// Enable Referrer-Policy header (default: true)
    /// </summary>
    public bool EnableReferrerPolicy { get; set; } = true;

    /// <summary>
    /// Referrer-Policy value (default: strict-origin-when-cross-origin)
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Enable Content-Security-Policy header (default: true)
    /// </summary>
    public bool EnableContentSecurityPolicy { get; set; } = true;

    /// <summary>
    /// Content-Security-Policy value
    /// </summary>
    public string ContentSecurityPolicy { get; set; } =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' https:; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    /// <summary>
    /// Enable HSTS (HTTP Strict Transport Security) header (default: true)
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// HSTS max age in seconds (default: 1 year = 31536000)
    /// </summary>
    public int HstsMaxAge { get; set; } = 31536000;

    /// <summary>
    /// Enable Permissions-Policy header (default: true)
    /// </summary>
    public bool EnablePermissionsPolicy { get; set; } = true;

    /// <summary>
    /// Permissions-Policy value
    /// </summary>
    public string PermissionsPolicy { get; set; } =
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=()";
}

/// <summary>
/// Extension methods for security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        Action<SecurityHeadersOptions>? configureOptions = null)
    {
        var options = new SecurityHeadersOptions();
        configureOptions?.Invoke(options);

        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}
