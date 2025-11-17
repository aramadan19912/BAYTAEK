using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Category;

public class CreateCategoryCommand : IRequest<Result<CategoryCreatedDto>>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid AdminUserId { get; set; }
}

public class CategoryCreatedDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
