using HomeService.Application.Commands.Review;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Review;

public class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, Result<ReviewDto>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<SubmitReviewCommandHandler> _logger;

    public SubmitReviewCommandHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<Domain.Entities.Review> reviewRepository,
        IRepository<Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        IUnitOfWork unitOfWork,
        IPushNotificationService pushNotificationService,
        ILogger<SubmitReviewCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<ReviewDto>.Failure("Booking not found");
            }

            // Verify customer owns this booking
            if (booking.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Customer {CustomerId} attempted to review booking {BookingId} they don't own",
                    request.CustomerId, request.BookingId);
                return Result<ReviewDto>.Failure("You are not authorized to review this booking");
            }

            // Verify booking is completed
            if (booking.Status != BookingStatus.Completed)
            {
                return Result<ReviewDto>.Failure("You can only review completed bookings");
            }

            // Check if review already exists
            var existingReviews = await _reviewRepository.FindAsync(
                r => r.BookingId == request.BookingId,
                cancellationToken);

            if (existingReviews != null && existingReviews.Any())
            {
                return Result<ReviewDto>.Failure("You have already reviewed this booking");
            }

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
            {
                return Result<ReviewDto>.Failure("Rating must be between 1 and 5 stars");
            }

            // Get customer info
            var customer = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                return Result<ReviewDto>.Failure("Customer not found");
            }

            // Create review
            var review = new Domain.Entities.Review
            {
                Id = Guid.NewGuid(),
                BookingId = request.BookingId,
                ServiceId = booking.ServiceId,
                ProviderId = booking.ProviderId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment,
                ImageUrls = request.ImageUrls?.ToArray(),
                IsAnonymous = request.IsAnonymous,
                IsVisible = true, // Auto-approve reviews (can add moderation later)
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CustomerId.ToString()
            };

            await _reviewRepository.AddAsync(review, cancellationToken);

            // Update provider's average rating and total reviews
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId, cancellationToken);
            if (provider != null)
            {
                var allProviderReviews = await _reviewRepository.FindAsync(
                    r => r.ProviderId == booking.ProviderId && r.IsVisible,
                    cancellationToken);

                var reviewsList = allProviderReviews?.ToList() ?? new List<Domain.Entities.Review>();
                reviewsList.Add(review); // Include the new review

                provider.TotalReviews = reviewsList.Count;
                provider.AverageRating = Math.Round((decimal)reviewsList.Average(r => r.Rating), 2);
                provider.UpdatedAt = DateTime.UtcNow;
                provider.UpdatedBy = request.CustomerId.ToString();

                await _providerRepository.UpdateAsync(provider, cancellationToken);
            }

            // Update service's average rating
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
            if (service != null)
            {
                var allServiceReviews = await _reviewRepository.FindAsync(
                    r => r.ServiceId == booking.ServiceId && r.IsVisible,
                    cancellationToken);

                var serviceReviewsList = allServiceReviews?.ToList() ?? new List<Domain.Entities.Review>();
                serviceReviewsList.Add(review); // Include the new review

                // Calculate average rating for the service
                var averageRating = Math.Round((decimal)serviceReviewsList.Average(r => r.Rating), 2);

                // TODO: Add AverageRating property to Service entity
                // service.AverageRating = averageRating;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = request.CustomerId.ToString();

                await _serviceRepository.UpdateAsync(service, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Review submitted for booking {BookingId} by customer {CustomerId}. Rating: {Rating}",
                request.BookingId, request.CustomerId, request.Rating);

            // Send notification to provider
            try
            {
                var message = request.Rating >= 4
                    ? $"You received a {request.Rating}-star review!"
                    : $"You received a new review";

                await _pushNotificationService.SendNotificationAsync(
                    booking.ProviderId,
                    "New Review",
                    message,
                    new Dictionary<string, string>
                    {
                        { "type", "new_review" },
                        { "reviewId", review.Id.ToString() },
                        { "bookingId", booking.Id.ToString() },
                        { "rating", request.Rating.ToString() }
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review notification to provider {ProviderId}", booking.ProviderId);
            }

            // Build response DTO
            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                BookingId = review.BookingId,
                ServiceId = review.ServiceId,
                ProviderId = review.ProviderId,
                CustomerName = review.IsAnonymous ? "Anonymous" : $"{customer.FirstName} {customer.LastName}",
                CustomerProfileImage = review.IsAnonymous ? null : customer.ProfileImageUrl,
                Rating = review.Rating,
                Comment = review.Comment,
                ImageUrls = review.ImageUrls?.ToList() ?? new List<string>(),
                IsAnonymous = review.IsAnonymous,
                ProviderResponse = review.ProviderResponse,
                ProviderResponseAt = review.ProviderResponseAt,
                IsVisible = review.IsVisible,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };

            return Result<ReviewDto>.Success(reviewDto, "Review submitted successfully. Thank you for your feedback!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for booking {BookingId}", request.BookingId);
            return Result<ReviewDto>.Failure("An error occurred while submitting the review");
        }
    }
}
