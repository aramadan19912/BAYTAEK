using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Content;

public class GetContentBySlugQuery : IRequest<Result<ContentDetailDto>>
{
    public string Slug { get; set; } = string.Empty;
}

public class ContentDetailDto
{
    public Guid Id { get; set; }
    public ContentType Type { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string ContentEn { get; set; } = string.Empty;
    public string ContentAr { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
}
