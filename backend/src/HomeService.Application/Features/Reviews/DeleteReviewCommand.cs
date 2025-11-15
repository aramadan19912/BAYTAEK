using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record DeleteReviewCommand(
    Guid ReviewId,
    Guid CustomerId
) : IRequest<Result<bool>>;

public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReviewCommandHandler> _logger;

    public DeleteReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get review
            var review = await _unitOfWork.Repository<Review>()
                .GetByIdAsync(request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<bool>("Review not found");

            // Validate ownership
            if (review.CustomerId != request.CustomerId)
                return Result.Failure<bool>("Unauthorized: You can only delete your own reviews");

            var providerId = review.ProviderId;

            // Delete review
            _unitOfWork.Repository<Review>().Delete(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update provider rating
            await UpdateProviderRating(providerId, cancellationToken);

            _logger.LogInformation("Review deleted: {ReviewId} by customer {CustomerId}",
                request.ReviewId, request.CustomerId);

            return Result.Success(true, "Review deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", request.ReviewId);
            return Result.Failure<bool>("An error occurred while deleting the review", ex.Message);
        }
    }

    private async Task UpdateProviderRating(Guid providerId, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.ProviderId == providerId && r.IsVisible)
            .ToListAsync(cancellationToken);

        var provider = await _unitOfWork.Repository<ServiceProvider>()
            .GetByIdAsync(providerId, cancellationToken);

        if (provider != null)
        {
            if (reviews.Any())
            {
                provider.AverageRating = Math.Round(reviews.Average(r => r.Rating), 2);
                provider.TotalReviews = reviews.Count;
            }
            else
            {
                provider.AverageRating = 0;
                provider.TotalReviews = 0;
            }

            _unitOfWork.Repository<ServiceProvider>().Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
