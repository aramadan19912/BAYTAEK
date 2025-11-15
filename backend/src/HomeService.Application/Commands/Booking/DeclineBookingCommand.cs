using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Booking;

public class DeclineBookingCommand : IRequest<Result<bool>>
{
    public Guid BookingId { get; set; }
    public Guid ProviderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
