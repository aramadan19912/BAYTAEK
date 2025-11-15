using HomeService.Application.Common;
using HomeService.Application.Mappings;
using MediatR;

namespace HomeService.Application.Commands.Reviews;

public record CreateReviewCommand : IRequest<Result<ReviewDto>>
{
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public string[] VideoUrls { get; init; } = Array.Empty<string>();
}
