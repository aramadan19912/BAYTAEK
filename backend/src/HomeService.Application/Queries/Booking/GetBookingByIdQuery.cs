using HomeService.Application.Commands.Booking;
using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.Booking;

public class GetBookingByIdQuery : IRequest<Result<BookingDetailDto>>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }  // For authorization check
}

public class BookingDetailDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;

    // Service Info
    public Guid ServiceId { get; set; }
    public string ServiceNameEn { get; set; } = string.Empty;
    public string ServiceNameAr { get; set; } = string.Empty;
    public string ServiceDescriptionEn { get; set; } = string.Empty;
    public string ServiceDescriptionAr { get; set; } = string.Empty;
    public List<string> ServiceImages { get; set; } = new();

    // Customer Info
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerProfileImage { get; set; }

    // Provider Info
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderBusinessName { get; set; }
    public string ProviderPhone { get; set; } = string.Empty;
    public string? ProviderProfileImage { get; set; }
    public decimal ProviderAverageRating { get; set; }
    public int ProviderTotalReviews { get; set; }

    // Location Info
    public Guid AddressId { get; set; }
    public string AddressLabel { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? AdditionalDirections { get; set; }

    // Booking Details
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? SpecialInstructions { get; set; }

    // Pricing
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal VatPercentage { get; set; }
    public string Currency { get; set; } = string.Empty;

    // Payment
    public bool IsPaid { get; set; }
    public Guid? PaymentId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? TransactionId { get; set; }

    // Review
    public Guid? ReviewId { get; set; }
    public int? Rating { get; set; }
    public string? ReviewComment { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Recurring Booking
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
}
