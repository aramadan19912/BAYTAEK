using HomeService.Application.Common;
using HomeService.Application.DTOs.Provider;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public record GetProviderDashboardQuery : IRequest<Result<ProviderDashboardDto>>
{
    public Guid ProviderId { get; init; }
}
