using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetFinancialAnalyticsQuery : IRequest<Result<FinancialAnalyticsDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Region? Region { get; init; }
}
