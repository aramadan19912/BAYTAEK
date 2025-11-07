using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs.Admin;

public class FinancialAnalyticsDto
{
    public RevenueBreakdown Revenue { get; set; } = new();
    public CommissionBreakdown Commission { get; set; } = new();
    public PayoutSummary Payouts { get; set; } = new();
    public RefundSummary Refunds { get; set; } = new();
    public List<DailyFinancialData> DailyData { get; set; } = new();
    public RegionalFinancials RegionalBreakdown { get; set; } = new();
    public PaymentMethodBreakdown PaymentMethods { get; set; } = new();
}

public class RevenueBreakdown
{
    public decimal TotalRevenue { get; set; }
    public decimal CompletedBookingsRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal GrowthRate { get; set; } // Compared to previous period
}

public class CommissionBreakdown
{
    public decimal TotalCommissionEarned { get; set; }
    public decimal CommissionRate { get; set; } = 0.18m; // 18%
    public decimal CommissionFromSaudi { get; set; }
    public decimal CommissionFromEgypt { get; set; }
    public int TotalCommissionableBookings { get; set; }
}

public class PayoutSummary
{
    public decimal TotalPayouts { get; set; }
    public decimal PendingPayouts { get; set; }
    public decimal ProcessedPayouts { get; set; }
    public int TotalProvidersPaid { get; set; }
    public int ProvidersAwaitingPayout { get; set; }
}

public class RefundSummary
{
    public decimal TotalRefunded { get; set; }
    public int TotalRefunds { get; set; }
    public decimal PendingRefunds { get; set; }
    public int PendingRefundCount { get; set; }
    public decimal RefundRate { get; set; } // Percentage of total revenue
}

public class DailyFinancialData
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Commission { get; set; }
    public decimal Payouts { get; set; }
    public decimal Refunds { get; set; }
    public int Transactions { get; set; }
}

public class RegionalFinancials
{
    public RegionRevenue SaudiArabia { get; set; } = new();
    public RegionRevenue Egypt { get; set; } = new();
}

public class RegionRevenue
{
    public decimal TotalRevenue { get; set; }
    public decimal Commission { get; set; }
    public decimal VatCollected { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class PaymentMethodBreakdown
{
    public decimal CreditCardRevenue { get; set; }
    public decimal WalletRevenue { get; set; }
    public decimal CashRevenue { get; set; }
    public int CreditCardTransactions { get; set; }
    public int WalletTransactions { get; set; }
    public int CashTransactions { get; set; }
}
