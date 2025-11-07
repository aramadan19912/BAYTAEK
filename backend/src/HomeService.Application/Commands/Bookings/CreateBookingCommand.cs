using HomeService.Application.Common;
using HomeService.Application.DTOs;
using MediatR;

namespace HomeService.Application.Commands.Bookings;

public record CreateBookingCommand : IRequest<Result<BookingDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid AddressId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public string? SpecialInstructions { get; init; }
    public bool IsRecurring { get; init; }
    public string? RecurrencePattern { get; init; }
}
