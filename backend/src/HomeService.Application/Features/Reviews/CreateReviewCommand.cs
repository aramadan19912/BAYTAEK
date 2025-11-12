using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record CreateReviewCommand(
    Guid BookingId,
    Guid CustomerId,
    int Rating,
    string? Comment = null,
    List<string>? ImageUrls = null,
    List<string>? VideoUrls = null
) : IRequest<Result<ReviewDto>>;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate rating range
            if (request.Rating < 1 || request.Rating > 5)
                return Result.Failure<ReviewDto>("Rating must be between 1 and 5");

            // Get booking with related data
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Provider)
                    .ThenInclude(p => p.User)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<ReviewDto>("Booking not found");

            // Validate customer ownership
            if (booking.CustomerId != request.CustomerId)
                return Result.Failure<ReviewDto>("Unauthorized: You can only review your own bookings");

            // Validate booking is completed
            if (booking.Status != BookingStatus.Completed)
                return Result.Failure<ReviewDto>("Can only review completed bookings");

            // Check if review already exists
            var existingReview = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .FirstOrDefaultAsync(r => r.BookingId == request.BookingId, cancellationToken);

            if (existingReview != null)
                return Result.Failure<ReviewDto>("Review already exists for this booking. Use update endpoint to modify it.");

            // Validate provider exists
            if (!booking.ProviderId.HasValue)
                return Result.Failure<ReviewDto>("Booking has no assigned provider");

            // Create review
            var review = new Review
            {
                BookingId = request.BookingId,
                CustomerId = request.CustomerId,
                ProviderId = booking.ProviderId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                ImageUrls = request.ImageUrls?.ToArray() ?? Array.Empty<string>(),
                VideoUrls = request.VideoUrls?.ToArray() ?? Array.Empty<string>(),
                IsVerified = true, // Auto-verified for now, can add moderation later
                IsVisible = true,
                HelpfulCount = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CustomerId.ToString()
            };

            await _unitOfWork.Repository<Review>().AddAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update provider's average rating
            await UpdateProviderRating(booking.ProviderId.Value, cancellationToken);

            // Send notification to provider
            _ = Task.Run(async () =>
            {
                try
                {
                    var titleEn = $"New {request.Rating}-Star Review";
                    var titleAr = $"تقييم جديد {request.Rating} نجوم";
                    var messageEn = $"{booking.Customer.FirstName} left a review for your service: {booking.Service.NameEn}";
                    var messageAr = $"{booking.Customer.FirstName} ترك تقييماً لخدمتك: {booking.Service.NameAr}";

                    await _notificationService.SendNotificationAsync(
                        booking.Provider!.UserId,
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
                    _logger.LogError(ex, "Error sending review notification");
                }
            }, cancellationToken);

            _logger.LogInformation("Review created: {ReviewId} for booking {BookingId} by customer {CustomerId}",
                review.Id, request.BookingId, request.CustomerId);

            var reviewDto = MapToDto(review);
            return Result.Success(reviewDto, "Review submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for booking {BookingId}", request.BookingId);
            return Result.Failure<ReviewDto>("An error occurred while creating the review", ex.Message);
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

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> VideoUrls { get; set; } = new();
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderRespondedAt { get; set; }
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Extended info for display
    public string? CustomerName { get; set; }
    public string? CustomerAvatarUrl { get; set; }
    public string? ServiceName { get; set; }
}
