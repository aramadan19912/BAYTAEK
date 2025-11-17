using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public class GetProviderActiveBookingsQuery : IRequest<Result<List<ProviderBookingDto>>>
{
    public Guid ProviderId { get; set; }
}

public class ProviderBookingDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;

    // Customer info
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerProfileImage { get; set; }

    // Service info
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    // Location info
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Schedule
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Pricing
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // Payment status
    public bool IsPaid { get; set; }

    // Special instructions
    public string? SpecialInstructions { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
}
