using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs.Admin;

public class AdminBookingListDto
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledDate { get; set; }
    public BookingStatus Status { get; set; }

    // Customer Information
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Provider Information
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string? ProviderEmail { get; set; }
    public string? ProviderPhone { get; set; }

    // Service Information
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    // Location and Pricing
    public Region Region { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PlatformCommission { get; set; }

    // Payment Status
    public bool IsPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Review Information
    public bool HasReview { get; set; }
    public decimal? ReviewRating { get; set; }
}
