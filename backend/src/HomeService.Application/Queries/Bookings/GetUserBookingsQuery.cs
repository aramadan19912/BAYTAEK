using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Bookings;

public record GetUserBookingsQuery : IRequest<Result<PagedResult<BookingDto>>>
{
    public Guid UserId { get; init; }
    public BookingStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
