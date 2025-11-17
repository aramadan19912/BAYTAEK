using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Category;

public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
    public bool? IsActive { get; set; } // Filter by active status
    public bool IncludeServiceCount { get; set; } = false; // Include count of services in each category
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int ServiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
