using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetAdminBookingsQuery : IRequest<Result<PagedResult<AdminBookingListDto>>>
{
    public BookingStatus? Status { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Region? Region { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ProviderId { get; init; }
    public Guid? ServiceId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsPaid { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
