using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.CustomerBooking;

public class GetCustomerBookingsQuery : IRequest<Result<CustomerBookingsDto>>
{
    public Guid CustomerId { get; set; }
    public BookingStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CustomerBookingsDto
{
    public List<CustomerBookingDto> Bookings { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Summary statistics
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal TotalSpent { get; set; }
}

public class CustomerBookingDto
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Service details
    public Guid ServiceId { get; set; }
    public string ServiceNameEn { get; set; } = string.Empty;
    public string ServiceNameAr { get; set; } = string.Empty;
    public string? ServiceImageUrl { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;

    // Provider details
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderProfileImageUrl { get; set; }
    public decimal ProviderRating { get; set; }
    public bool IsProviderVerified { get; set; }

    // Address details
    public Guid AddressId { get; set; }
    public string AddressLabel { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;

    // Pricing
    public decimal ServicePrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // Payment
    public bool IsPaid { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    // Review
    public bool HasReviewed { get; set; }

    // Additional info
    public string? CustomerNotes { get; set; }
    public string? ProviderNotes { get; set; }
}
