namespace HomeService.Application.DTOs.Provider;

public class ProviderEarningsDto
{
    public decimal TotalEarnings { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingEarnings { get; set; }
    public decimal ThisWeekEarnings { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public PayoutInfo? LastPayout { get; set; }
    public DateTime? NextPayoutDate { get; set; }
    public EarningsBreakdown Breakdown { get; set; } = new();
}

public class PayoutInfo
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EarningsBreakdown
{
    public List<CategoryEarnings> ByCategory { get; set; } = new();
    public List<DailyEarning> Daily { get; set; } = new();
    public PeakTimes PeakEarningTimes { get; set; } = new();
    public decimal AveragePerJob { get; set; }
    public decimal TotalTips { get; set; }
}

public class CategoryEarnings
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int JobCount { get; set; }
    public decimal Percentage { get; set; }
}

public class PeakTimes
{
    public List<string> PeakDays { get; set; } = new();
    public List<string> PeakHours { get; set; } = new();
}

public class ProviderTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceProvided { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal CommissionDeducted { get; set; }
    public decimal NetEarnings { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PayoutRequestDto
{
    public Guid PayoutId { get; set; }
    public decimal Amount { get; set; }
    public string BankAccount { get; set; } = string.Empty; // Masked
    public DateTime InitiatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsInstant { get; set; }
    public decimal Fee { get; set; }
}
