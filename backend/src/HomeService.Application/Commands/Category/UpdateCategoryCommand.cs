using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Category;

public class UpdateCategoryCommand : IRequest<Result<bool>>
{
    public Guid CategoryId { get; set; }
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? IconUrl { get; set; }
    public bool? IsActive { get; set; }
    public Guid AdminUserId { get; set; }
}
