using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.CustomerBooking;

public class GetBookingDetailQuery : IRequest<Result<CustomerBookingDetailDto>>
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; } // For authorization
}

public class CustomerBookingDetailDto
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Service details
    public ServiceInfoDto Service { get; set; } = new();

    // Provider details
    public ProviderInfoDto Provider { get; set; } = new();

    // Address details
    public AddressInfoDto Address { get; set; } = new();

    // Pricing breakdown
    public PricingInfoDto Pricing { get; set; } = new();

    // Payment details
    public PaymentInfoDto? Payment { get; set; }

    // Review details
    public ReviewInfoDto? Review { get; set; }

    // Timeline
    public List<BookingTimelineDto> Timeline { get; set; } = new();

    // Notes
    public string? CustomerNotes { get; set; }
    public string? ProviderNotes { get; set; }
    public string? AdminNotes { get; set; }

    // Photos (before/after)
    public List<string> BeforePhotos { get; set; } = new();
    public List<string> AfterPhotos { get; set; } = new();

    // Cancellation info
    public string? CancellationReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public RefundStatus? RefundStatus { get; set; }
}

public class ServiceInfoDto
{
    public Guid ServiceId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int EstimatedDurationMinutes { get; set; }
}

public class ProviderInfoDto
{
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsVerified { get; set; }
    public int YearsOfExperience { get; set; }
}

public class AddressInfoDto
{
    public Guid AddressId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class PricingInfoDto
{
    public decimal ServicePrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";
}

public class PaymentInfoDto
{
    public Guid PaymentId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
}

public class ReviewInfoDto
{
    public Guid ReviewId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderRespondedAt { get; set; }
}

public class BookingTimelineDto
{
    public BookingStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
}
