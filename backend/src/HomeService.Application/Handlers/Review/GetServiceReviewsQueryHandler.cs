using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Review;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Review;

public class GetServiceReviewsQueryHandler : IRequestHandler<GetServiceReviewsQuery, Result<ServiceReviewsDto>>
{
    private readonly IRepository<Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly ILogger<GetServiceReviewsQueryHandler> _logger;

    public GetServiceReviewsQueryHandler(
        IRepository<Domain.Entities.Review> reviewRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        IRepository<Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        ILogger<GetServiceReviewsQueryHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<Result<ServiceReviewsDto>> Handle(GetServiceReviewsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get service
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result<ServiceReviewsDto>.Failure("Service not found");
            }

            // Validate pagination
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            // Get all reviews for this service
            var allReviews = await _reviewRepository.FindAsync(
                r => r.ServiceId == request.ServiceId && r.IsVisible,
                cancellationToken);

            var reviewsList = allReviews?.ToList() ?? new List<Domain.Entities.Review>();

            // Apply filters
            if (request.MinRating.HasValue && request.MinRating.Value >= 1 && request.MinRating.Value <= 5)
            {
                reviewsList = reviewsList.Where(r => r.Rating >= request.MinRating.Value).ToList();
            }

            if (request.WithImagesOnly)
            {
                reviewsList = reviewsList.Where(r => r.ImageUrls != null && r.ImageUrls.Length > 0).ToList();
            }

            // Calculate summary statistics
            var totalReviews = reviewsList.Count;
            var averageRating = totalReviews > 0 ? Math.Round((decimal)reviewsList.Average(r => r.Rating), 2) : 0;

            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviewsList.Count(r => r.Rating == i);
            }

            var reviewsWithImages = reviewsList.Count(r => r.ImageUrls != null && r.ImageUrls.Length > 0);
            var reviewsWithComments = reviewsList.Count(r => !string.IsNullOrWhiteSpace(r.Comment));

            // Sort by most recent
            var sortedReviews = reviewsList.OrderByDescending(r => r.CreatedAt).ToList();

            // Apply pagination
            var totalCount = sortedReviews.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedReviews = sortedReviews
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Get related data for reviews (customers, providers)
            var customerIds = pagedReviews.Select(r => r.CustomerId).Distinct().ToList();
            var providerIds = pagedReviews.Select(r => r.ProviderId).Distinct().ToList();

            var customers = await _userRepository.FindAsync(
                u => customerIds.Contains(u.Id),
                cancellationToken);

            var providers = await _providerRepository.FindAsync(
                p => providerIds.Contains(p.Id),
                cancellationToken);

            var customersDict = customers?.ToDictionary(c => c.Id) ?? new Dictionary<Guid, Domain.Entities.User>();
            var providersDict = providers?.ToDictionary(p => p.Id) ?? new Dictionary<Guid, ServiceProvider>();

            // Build review DTOs
            var reviewDtos = pagedReviews.Select(review =>
            {
                var customer = customersDict.GetValueOrDefault(review.CustomerId);
                var provider = providersDict.GetValueOrDefault(review.ProviderId);

                return new ReviewDetailDto
                {
                    Id = review.Id,
                    CustomerName = review.IsAnonymous ? "Anonymous" :
                        customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                    CustomerProfileImage = review.IsAnonymous ? null : customer?.ProfileImageUrl,
                    ProviderName = provider?.BusinessName ?? "",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    ImageUrls = review.ImageUrls?.ToList() ?? new List<string>(),
                    IsAnonymous = review.IsAnonymous,
                    ProviderResponse = review.ProviderResponse,
                    ProviderResponseAt = review.ProviderResponseAt,
                    ServiceName = service.NameEn,
                    CreatedAt = review.CreatedAt,
                    IsHelpful = false, // TODO: Implement helpful voting
                    HelpfulCount = 0 // TODO: Implement helpful count
                };
            }).ToList();

            var responseDto = new ServiceReviewsDto
            {
                ServiceId = request.ServiceId,
                ServiceName = service.NameEn,
                Summary = new ReviewsSummaryDto
                {
                    AverageRating = averageRating,
                    TotalReviews = totalReviews,
                    RatingDistribution = ratingDistribution,
                    ReviewsWithImages = reviewsWithImages,
                    ReviewsWithComments = reviewsWithComments
                },
                Reviews = reviewDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            _logger.LogInformation("Retrieved {Count} reviews for service {ServiceId}. Page: {PageNumber}",
                reviewDtos.Count, request.ServiceId, request.PageNumber);

            return Result<ServiceReviewsDto>.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for service {ServiceId}", request.ServiceId);
            return Result<ServiceReviewsDto>.Failure("An error occurred while retrieving reviews");
        }
    }
}
