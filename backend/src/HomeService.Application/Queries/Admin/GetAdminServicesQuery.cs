using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetAdminServicesQuery : IRequest<Result<PagedResult<AdminServiceListDto>>>
{
    public Guid? CategoryId { get; init; }
    public Region? Region { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
