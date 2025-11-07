using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Bookings;

public record UpdateBookingStatusCommand : IRequest<Result>
{
    public Guid BookingId { get; init; }
    public BookingStatus NewStatus { get; init; }
    public Guid UserId { get; init; }
    public string? CancellationReason { get; init; }
}
