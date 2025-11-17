using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Booking;

public class CancelBookingCommand : IRequest<Result<bool>>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }  // For authorization
    public string Reason { get; set; } = string.Empty;
    public bool IsCustomerCancellation { get; set; } = true;
}
