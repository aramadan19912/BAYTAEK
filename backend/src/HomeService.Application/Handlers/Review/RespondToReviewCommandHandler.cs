using HomeService.Application.Commands.Review;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Review;

public class RespondToReviewCommandHandler : IRequestHandler<RespondToReviewCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Review> _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<RespondToReviewCommandHandler> _logger;

    public RespondToReviewCommandHandler(
        IRepository<Domain.Entities.Review> reviewRepository,
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotificationService,
        ILogger<RespondToReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get review
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
            if (review == null)
            {
                return Result<bool>.Failure("Review not found");
            }

            // Verify provider owns this review (the review is for their service)
            if (review.ProviderId != request.ProviderId)
            {
                _logger.LogWarning("Provider {ProviderId} attempted to respond to review {ReviewId} they don't own",
                    request.ProviderId, request.ReviewId);
                return Result<bool>.Failure("You are not authorized to respond to this review");
            }

            // Validate response
            if (string.IsNullOrWhiteSpace(request.Response))
            {
                return Result<bool>.Failure("Response cannot be empty");
            }

            if (request.Response.Length > 1000)
            {
                return Result<bool>.Failure("Response cannot exceed 1000 characters");
            }

            // Check if provider already responded
            if (!string.IsNullOrWhiteSpace(review.ProviderResponse))
            {
                return Result<bool>.Failure("You have already responded to this review. You can only respond once.");
            }

            // Add provider response
            review.ProviderResponse = request.Response.Trim();
            review.ProviderResponseAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.ProviderId.ToString();

            await _reviewRepository.UpdateAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Provider {ProviderId} responded to review {ReviewId}",
                request.ProviderId, request.ReviewId);

            // Send notification to customer
            try
            {
                await _pushNotificationService.SendNotificationAsync(
                    review.CustomerId,
                    "Provider Response",
                    "The provider has responded to your review",
                    new Dictionary<string, string>
                    {
                        { "type", "review_response" },
                        { "reviewId", review.Id.ToString() },
                        { "bookingId", review.BookingId.ToString() }
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review response notification to customer {CustomerId}", review.CustomerId);
            }

            return Result<bool>.Success(true, "Response posted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", request.ReviewId);
            return Result<bool>.Failure("An error occurred while posting the response");
        }
    }
}
