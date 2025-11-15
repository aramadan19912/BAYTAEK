using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.Content;

public class GetFAQsQuery : IRequest<Result<FAQListDto>>
{
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
}

public class FAQListDto
{
    public List<FAQDto> FAQs { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public int TotalCount { get; set; }
}

public class FAQDto
{
    public Guid Id { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string QuestionAr { get; set; } = string.Empty;
    public string AnswerEn { get; set; } = string.Empty;
    public string AnswerAr { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
}
