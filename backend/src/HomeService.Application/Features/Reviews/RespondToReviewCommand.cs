using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record RespondToReviewCommand(
    Guid ReviewId,
    Guid ProviderId,
    string Response
) : IRequest<Result<ReviewDto>>;

public class RespondToReviewCommandHandler : IRequestHandler<RespondToReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RespondToReviewCommandHandler> _logger;

    public RespondToReviewCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<RespondToReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate response is not empty
            if (string.IsNullOrWhiteSpace(request.Response))
                return Result.Failure<ReviewDto>("Response cannot be empty");

            // Get review with related data
            var review = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                    .ThenInclude(p => p.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<ReviewDto>("Review not found");

            // Validate provider ownership
            if (review.ProviderId != request.ProviderId)
                return Result.Failure<ReviewDto>("Unauthorized: You can only respond to reviews for your services");

            // Check if already responded
            if (!string.IsNullOrEmpty(review.ProviderResponse))
                return Result.Failure<ReviewDto>("You have already responded to this review. Contact support to modify your response.");

            // Update review with response
            review.ProviderResponse = request.Response;
            review.ProviderRespondedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            review.UpdatedBy = request.ProviderId.ToString();

            _unitOfWork.Repository<Review>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to customer
            _ = Task.Run(async () =>
            {
                try
                {
                    var titleEn = "Provider Responded to Your Review";
                    var titleAr = "رد مقدم الخدمة على تقييمك";
                    var messageEn = $"{review.Provider.User.FirstName} responded to your review for {review.Booking.Service.NameEn}";
                    var messageAr = $"رد {review.Provider.User.FirstName} على تقييمك لخدمة {review.Booking.Service.NameAr}";

                    await _notificationService.SendNotificationAsync(
                        review.CustomerId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Booking,
                        review.Id,
                        nameof(Review),
                        $"/reviews/{review.Id}",
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending provider response notification");
                }
            }, cancellationToken);

            _logger.LogInformation("Provider {ProviderId} responded to review {ReviewId}",
                request.ProviderId, request.ReviewId);

            var reviewDto = MapToDto(review);
            return Result.Success(reviewDto, "Response submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", request.ReviewId);
            return Result.Failure<ReviewDto>("An error occurred while submitting your response", ex.Message);
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
