using Hangfire.Dashboard;

namespace HomeService.API;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all
        // In production, you should implement proper authorization
        var httpContext = context.GetHttpContext();

        // For production, check if user is authenticated and has Admin role
        // return httpContext.User.Identity?.IsAuthenticated == true &&
        //        httpContext.User.IsInRole("Admin");

        return true; // Allow for development
    }
}
