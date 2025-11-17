using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Content;

public class GetContentByTypeQuery : IRequest<Result<ContentListDto>>
{
    public ContentType Type { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public bool? IsPublished { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ContentListDto
{
    public List<ContentSummaryDto> Contents { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ContentSummaryDto
{
    public Guid Id { get; set; }
    public ContentType Type { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
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
}
