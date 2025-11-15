using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.ProviderAnalytics;

public class GetProviderAnalyticsQuery : IRequest<Result<ProviderAnalyticsDto>>
{
    public Guid ProviderId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ProviderAnalyticsDto
{
    // Earnings
    public EarningsAnalyticsDto Earnings { get; set; } = new();

    // Bookings
    public BookingAnalyticsDto Bookings { get; set; } = new();

    // Performance
    public PerformanceAnalyticsDto Performance { get; set; } = new();

    // Customer Insights
    public CustomerInsightsDto Customers { get; set; } = new();

    // Services
    public List<ServicePerformanceDto> TopServices { get; set; } = new();

    // Trends (daily breakdown)
    public List<DailyTrendDto> DailyTrends { get; set; } = new();
}

public class EarningsAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetEarnings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public decimal ProjectedMonthlyEarnings { get; set; }

    // Breakdown by status
    public decimal CompletedBookingsRevenue { get; set; }
    public decimal PendingPayoutsAmount { get; set; }
    public decimal PaidOutAmount { get; set; }

    // Growth
    public decimal RevenueGrowthPercentage { get; set; }
    public decimal ComparisonPeriodRevenue { get; set; }
}

public class BookingAnalyticsDto
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int InProgressBookings { get; set; }
    public int PendingBookings { get; set; }

    public decimal CompletionRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal AverageResponseTime { get; set; } // in hours

    // Growth
    public int BookingGrowthCount { get; set; }
    public decimal BookingGrowthPercentage { get; set; }
    public int ComparisonPeriodBookings { get; set; }
}

public class PerformanceAnalyticsDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarReviews { get; set; }
    public int FourStarReviews { get; set; }
    public int ThreeStarReviews { get; set; }
    public int TwoStarReviews { get; set; }
    public int OneStarReviews { get; set; }

    public decimal ResponseRate { get; set; } // Percentage of bookings responded to
    public decimal OnTimeCompletionRate { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }

    // Comparison
    public decimal RatingGrowth { get; set; }
    public decimal ComparisonPeriodRating { get; set; }
}

public class CustomerInsightsDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal RetentionRate { get; set; }
    public decimal AverageCustomerLifetimeValue { get; set; }

    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

public class TopCustomerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class ServicePerformanceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceNameEn { get; set; } = string.Empty;
    public string ServiceNameAr { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
}

public class DailyTrendDto
{
    public DateTime Date { get; set; }
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
}
