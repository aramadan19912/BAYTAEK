using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Content;

public class CreateContentCommand : IRequest<Result<ContentCreatedDto>>
{
    public Guid AdminUserId { get; set; }
    public ContentType Type { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string ContentEn { get; set; } = string.Empty;
    public string ContentAr { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool IsPublished { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}

public class ContentCreatedDto
{
    public Guid ContentId { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public bool IsPublished { get; set; }
    public string Message { get; set; } = string.Empty;
}
