using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Booking;

public class UpdateBookingStatusCommand : IRequest<Result<BookingDto>>
{
    public Guid BookingId { get; set; }
    public Guid ProviderId { get; set; }
    public string Status { get; set; } = string.Empty; // on_the_way, arrived, in_progress, completed
    public string? Notes { get; set; }
    public List<string>? PhotoUrls { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // Service details
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceNameAr { get; set; } = string.Empty;

    // Customer details
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    // Address details
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
