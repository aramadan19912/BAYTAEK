using HomeService.Application.Common;
using HomeService.Application.Mappings;
using MediatR;

namespace HomeService.Application.Queries.Services;

public record GetServiceCategoriesQuery : IRequest<Result<List<ServiceCategoryDto>>>
{
    public Guid? ParentCategoryId { get; init; }
    public bool IncludeInactive { get; init; } = false;
}
