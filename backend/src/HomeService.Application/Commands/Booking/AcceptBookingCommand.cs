using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Booking;

public class AcceptBookingCommand : IRequest<Result<BookingDto>>
{
    public Guid BookingId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public string? Notes { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? EstimatedArrival { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
    public string? Notes { get; set; }
}
