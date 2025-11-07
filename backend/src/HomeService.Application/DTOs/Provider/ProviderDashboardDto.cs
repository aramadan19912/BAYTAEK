namespace HomeService.Application.DTOs.Provider;

public class ProviderDashboardDto
{
    public TodaySummary Today { get; set; } = new();
    public QuickStats Stats { get; set; } = new();
    public bool IsOnline { get; set; }
    public List<UpcomingJob> UpcomingJobs { get; set; } = new();
    public EarningsChart SevenDayEarnings { get; set; } = new();
}

public class TodaySummary
{
    public int TodaysBookings { get; set; }
    public decimal EarningsToday { get; set; }
    public int PendingApprovals { get; set; }
    public decimal CurrentRating { get; set; }
}

public class QuickStats
{
    public decimal ThisWeekEarnings { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public int TotalJobsCompleted { get; set; }
    public decimal AcceptanceRate { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
}

public class UpcomingJob
{
    public Guid BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhoto { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; } // in km
    public decimal Earnings { get; set; }
}

public class EarningsChart
{
    public List<DailyEarning> DailyEarnings { get; set; } = new();
    public decimal TotalSevenDays { get; set; }
    public decimal PreviousSevenDays { get; set; }
    public decimal ChangePercentage { get; set; }
}

public class DailyEarning
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int JobCount { get; set; }
}
