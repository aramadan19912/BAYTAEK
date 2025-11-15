using HomeService.Application.Common;
using HomeService.Application.DTOs.Provider;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public record GetProviderEarningsQuery : IRequest<Result<ProviderEarningsDto>>
{
    public Guid ProviderId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
