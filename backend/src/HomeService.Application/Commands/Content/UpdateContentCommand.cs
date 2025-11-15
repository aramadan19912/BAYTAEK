using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Content;

public class UpdateContentCommand : IRequest<Result<bool>>
{
    public Guid ContentId { get; set; }
    public Guid AdminUserId { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? ContentEn { get; set; }
    public string? ContentAr { get; set; }
    public string? Slug { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool? IsPublished { get; set; }
    public int? DisplayOrder { get; set; }
}
