using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record UpdateReviewCommand(
    Guid ReviewId,
    Guid CustomerId,
    int Rating,
    string? Comment = null,
    List<string>? ImageUrls = null,
    List<string>? VideoUrls = null
) : IRequest<Result<ReviewDto>>;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;

    public UpdateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate rating range
            if (request.Rating < 1 || request.Rating > 5)
                return Result.Failure<ReviewDto>("Rating must be between 1 and 5");

            // Get review
            var review = await _unitOfWork.Repository<Review>()
                .GetByIdAsync(request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<ReviewDto>("Review not found");

            // Validate ownership
            if (review.CustomerId != request.CustomerId)
                return Result.Failure<ReviewDto>("Unauthorized: You can only update your own reviews");

            // Check if review can be edited (within 48 hours of creation)
            var hoursSinceCreation = (DateTime.UtcNow - review.CreatedAt).TotalHours;
            if (hoursSinceCreation > 48)
                return Result.Failure<ReviewDto>("Reviews can only be edited within 48 hours of creation");

            // Update review
            var oldRating = review.Rating;
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.ImageUrls = request.ImageUrls?.ToArray() ?? Array.Empty<string>();
            review.VideoUrls = request.VideoUrls?.ToArray() ?? Array.Empty<string>();
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.CustomerId.ToString();

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update provider rating if rating changed
            if (oldRating != request.Rating)
            {
                await UpdateProviderRating(review.ProviderId, cancellationToken);
            }

            _logger.LogInformation("Review updated: {ReviewId} by customer {CustomerId}",
                request.ReviewId, request.CustomerId);

            var reviewDto = MapToDto(review);
            return Result.Success(reviewDto, "Review updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", request.ReviewId);
            return Result.Failure<ReviewDto>("An error occurred while updating the review", ex.Message);
        }
    }

    private async Task UpdateProviderRating(Guid providerId, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.ProviderId == providerId && r.IsVisible)
            .ToListAsync(cancellationToken);

        if (reviews.Any())
        {
            var averageRating = reviews.Average(r => r.Rating);
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(providerId, cancellationToken);

            if (provider != null)
            {
                provider.AverageRating = Math.Round(averageRating, 2);
                provider.TotalReviews = reviews.Count;
                _unitOfWork.Repository<ServiceProvider>().Update(provider);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private ReviewDto MapToDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            BookingId = review.BookingId,
            CustomerId = review.CustomerId,
            ProviderId = review.ProviderId,
            Rating = review.Rating,
            Comment = review.Comment,
            ImageUrls = review.ImageUrls.ToList(),
            VideoUrls = review.VideoUrls.ToList(),
            ProviderResponse = review.ProviderResponse,
            ProviderRespondedAt = review.ProviderRespondedAt,
            IsVerified = review.IsVerified,
            HelpfulCount = review.HelpfulCount,
            CreatedAt = review.CreatedAt
        };
    }
}
