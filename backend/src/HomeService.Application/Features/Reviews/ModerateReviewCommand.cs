using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Reviews;

public record ModerateReviewCommand(
    Guid ReviewId,
    Guid AdminUserId,
    ReviewModerationAction Action,
    string? Reason = null
) : IRequest<Result<ReviewDto>>;

public enum ReviewModerationAction
{
    Approve,
    Reject,
    Hide,
    Show
}

public class ModerateReviewCommandHandler : IRequestHandler<ModerateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ModerateReviewCommandHandler> _logger;

    public ModerateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ModerateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(ModerateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get review
            var review = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<ReviewDto>("Review not found");

            var previousState = new { review.IsVerified, review.IsVisible };

            // Apply moderation action
            switch (request.Action)
            {
                case ReviewModerationAction.Approve:
                    review.IsVerified = true;
                    review.IsVisible = true;
                    break;

                case ReviewModerationAction.Reject:
                    review.IsVerified = false;
                    review.IsVisible = false;
                    break;

                case ReviewModerationAction.Hide:
                    review.IsVisible = false;
                    break;

                case ReviewModerationAction.Show:
                    review.IsVisible = true;
                    break;
            }

            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.AdminUserId.ToString();

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update provider rating if visibility changed
            if (previousState.IsVisible != review.IsVisible)
            {
                await UpdateProviderRating(review.ProviderId, cancellationToken);
            }

            // Send notification to customer if review was rejected
            if (request.Action == ReviewModerationAction.Reject)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var titleEn = "Review Not Approved";
                        var titleAr = "لم تتم الموافقة على التقييم";
                        var reasonText = !string.IsNullOrEmpty(request.Reason)
                            ? $"Reason: {request.Reason}"
                            : "Your review did not meet our community guidelines.";
                        var messageEn = $"Your recent review was not approved. {reasonText}";
                        var messageAr = $"لم تتم الموافقة على تقييمك الأخير. {reasonText}";

                        await _notificationService.SendNotificationAsync(
                            review.CustomerId,
                            titleEn,
                            titleAr,
                            messageEn,
                            messageAr,
                            NotificationCategory.System,
                            review.Id,
                            nameof(Review),
                            $"/reviews/{review.Id}",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending moderation notification");
                    }
                }, cancellationToken);
            }

            _logger.LogInformation("Review {ReviewId} moderated by admin {AdminId}: Action={Action}",
                request.ReviewId, request.AdminUserId, request.Action);

            var reviewDto = MapToDto(review);
            return Result.Success(reviewDto, $"Review {request.Action.ToString().ToLower()}ed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating review {ReviewId}", request.ReviewId);
            return Result.Failure<ReviewDto>("An error occurred while moderating the review", ex.Message);
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
