using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
