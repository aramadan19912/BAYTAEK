using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Review;

public class RespondToReviewCommand : IRequest<Result<bool>>
{
    public Guid ReviewId { get; set; }
    public Guid ProviderId { get; set; }
    public string Response { get; set; } = string.Empty;
}
