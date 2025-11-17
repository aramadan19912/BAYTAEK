using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Provider;

public class RequestPayoutCommand : IRequest<Result<PayoutRequestDto>>
{
    public Guid ProviderId { get; set; }
    public decimal Amount { get; set; }
    public bool IsInstant { get; set; } // Instant payout (with fee) vs Standard (2-3 days)
}

public class PayoutRequestDto
{
    public Guid PayoutId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal NetAmount { get; set; }
    public string PayoutMethod { get; set; } = string.Empty; // "instant" or "standard"
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string Message { get; set; } = string.Empty;
}
