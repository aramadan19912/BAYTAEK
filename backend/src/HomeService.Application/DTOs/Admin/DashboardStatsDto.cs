namespace HomeService.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public UserStats Users { get; set; } = new();
    public BookingStats Bookings { get; set; } = new();
    public RevenueStats Revenue { get; set; } = new();
    public ProviderStats Providers { get; set; } = new();
    public SystemStats System { get; set; } = new();
    public List<RecentBooking> RecentBookings { get; set; } = new();
    public List<TopService> TopServices { get; set; } = new();
    public RegionalStats RegionalBreakdown { get; set; } = new();
}

public class UserStats
{
    public int TotalCustomers { get; set; }
    public int TotalProviders { get; set; }
    public int ActiveToday { get; set; }
    public int NewThisWeek { get; set; }
    public int NewThisMonth { get; set; }
    public decimal GrowthRate { get; set; }
}

public class BookingStats
{
    public int TotalToday { get; set; }
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Disputed { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal CancellationRate { get; set; }
}

public class RevenueStats
{
    public decimal Today { get; set; }
    public decimal ThisWeek { get; set; }
    public decimal ThisMonth { get; set; }
    public decimal ThisYear { get; set; }
    public decimal PlatformCommission { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailyRevenue> Last30Days { get; set; } = new();
}

public class DailyRevenue
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class ProviderStats
{
    public int TotalActive { get; set; }
    public int TotalOffline { get; set; }
    public int PendingVerification { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class SystemStats
{
    public int PendingDisputes { get; set; }
    public int OpenTickets { get; set; }
    public string ApiStatus { get; set; } = "Healthy";
    public string DatabaseStatus { get; set; } = "Healthy";
}

public class RecentBooking
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TopService
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RegionalStats
{
    public RegionData SaudiArabia { get; set; } = new();
    public RegionData Egypt { get; set; } = new();
}

public class RegionData
{
    public int Users { get; set; }
    public int Bookings { get; set; }
    public decimal Revenue { get; set; }
    public int ActiveProviders { get; set; }
}
