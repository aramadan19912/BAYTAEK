using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record MarkReviewHelpfulCommand(
    Guid ReviewId,
    Guid UserId
) : IRequest<Result<int>>;

public class MarkReviewHelpfulCommandHandler : IRequestHandler<MarkReviewHelpfulCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkReviewHelpfulCommandHandler> _logger;

    public MarkReviewHelpfulCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<MarkReviewHelpfulCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(MarkReviewHelpfulCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get review
            var review = await _unitOfWork.Repository<Review>()
                .GetByIdAsync(request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<int>("Review not found");

            if (!review.IsVisible)
                return Result.Failure<int>("Review is not visible");

            // Increment helpful count
            review.HelpfulCount++;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review {ReviewId} marked as helpful by user {UserId}. New count: {Count}",
                request.ReviewId, request.UserId, review.HelpfulCount);

            return Result.Success(review.HelpfulCount, "Review marked as helpful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking review {ReviewId} as helpful", request.ReviewId);
            return Result.Failure<int>("An error occurred while marking the review as helpful", ex.Message);
        }
    }
}
