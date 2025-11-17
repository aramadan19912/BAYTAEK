using HomeService.Application.Common;
using HomeService.Application.DTOs;
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
