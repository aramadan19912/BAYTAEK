using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Services;

public record GetServicesQuery : IRequest<Result<PagedResult<ServiceDto>>>
{
    public Guid? CategoryId { get; init; }
    public Region? Region { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsFeatured { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
