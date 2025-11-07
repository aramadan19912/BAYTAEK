using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public class GetPayoutHistoryQuery : IRequest<Result<PayoutHistoryDto>>
{
    public Guid ProviderId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PayoutHistoryDto
{
    public List<PayoutDto> Payouts { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public PayoutSummaryDto Summary { get; set; } = new();
}

public class PayoutDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal NetAmount { get; set; }
    public string PayoutMethod { get; set; } = string.Empty; // "instant" or "standard"
    public string Status { get; set; } = string.Empty; // "pending", "processing", "completed", "failed"
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string? BankAccountLast4 { get; set; }
    public string? BankName { get; set; }
    public string? FailureReason { get; set; }
}

public class PayoutSummaryDto
{
    public decimal TotalPaidOut { get; set; }
    public decimal PendingPayouts { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TotalPayoutCount { get; set; }
    public decimal TotalFees { get; set; }
}
