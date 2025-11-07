using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.Service;
using HomeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Service;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDetailDto>>
{
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ILogger<GetServiceByIdQueryHandler> _logger;

    public GetServiceByIdQueryHandler(
        IRepository<Domain.Entities.Service> serviceRepository,
        IRepository<ServiceCategory> categoryRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<User> userRepository,
        IRepository<Review> reviewRepository,
        ILogger<GetServiceByIdQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<ServiceDetailDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get service
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result<ServiceDetailDto>.Failure("Service not found");
            }

            // Check if service is active (optionally, you might want to show inactive services to admins)
            if (!service.IsActive)
            {
                return Result<ServiceDetailDto>.Failure("This service is currently unavailable");
            }

            // Get category
            var category = await _categoryRepository.GetByIdAsync(service.CategoryId, cancellationToken);

            // Get provider info
            var provider = await _providerRepository.FindAsync(
                p => p.Id == service.ProviderId,
                cancellationToken);
            var providerEntity = provider?.FirstOrDefault();

            // Get provider user info
            User? providerUser = null;
            if (providerEntity != null)
            {
                providerUser = await _userRepository.GetByIdAsync(providerEntity.UserId, cancellationToken);
            }

            // Get reviews for this service
            var allReviews = await _reviewRepository.FindAsync(
                r => r.ServiceId == service.Id && r.IsVisible,
                cancellationToken);

            var reviewsList = allReviews?.OrderByDescending(r => r.CreatedAt).ToList() ?? new List<Review>();

            // Calculate average rating
            var averageRating = reviewsList.Any()
                ? reviewsList.Average(r => r.Rating)
                : 0;

            // Get recent reviews (top 5)
            var recentReviews = reviewsList
                .Take(5)
                .Select(r => new ServiceReviewDto
                {
                    Id = r.Id,
                    CustomerName = "Customer", // TODO: Get customer name from User table
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ImageUrls = r.ImageUrls?.ToList() ?? new List<string>(),
                    CreatedAt = r.CreatedAt,
                    ProviderResponse = r.ProviderResponse
                })
                .ToList();

            // Build DTO
            var dto = new ServiceDetailDto
            {
                Id = service.Id,
                NameEn = service.NameEn,
                NameAr = service.NameAr,
                DescriptionEn = service.DescriptionEn,
                DescriptionAr = service.DescriptionAr,

                CategoryId = service.CategoryId,
                CategoryNameEn = category?.NameEn ?? "Unknown Category",
                CategoryNameAr = category?.NameAr ?? "فئة غير معروفة",

                ProviderId = service.ProviderId,
                ProviderName = providerUser != null
                    ? $"{providerUser.FirstName} {providerUser.LastName}"
                    : "Unknown Provider",
                ProviderBusinessName = providerEntity?.BusinessName,
                ProviderAverageRating = providerEntity?.AverageRating ?? 0,
                ProviderTotalReviews = providerEntity?.TotalReviews ?? 0,
                ProviderCompletedBookings = providerEntity?.CompletedBookings ?? 0,
                ProviderIsVerified = providerEntity?.IsVerified ?? false,

                BasePrice = service.BasePrice,
                Currency = service.Currency,
                EstimatedDurationMinutes = service.EstimatedDurationMinutes,
                IsActive = service.IsActive,
                IsFeatured = service.IsFeatured,

                ImageUrls = service.ImageUrls?.ToList() ?? new List<string>(),
                VideoUrl = service.VideoUrl,

                AvailableRegions = service.AvailableRegions?.Select(r => r.ToString()).ToList() ?? new List<string>(),

                RequiredMaterials = service.RequiredMaterials,
                WarrantyInfo = service.WarrantyInfo,

                RecentReviews = recentReviews,
                TotalReviews = reviewsList.Count,
                AverageRating = averageRating,

                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };

            _logger.LogInformation("Retrieved service details for {ServiceId}", request.ServiceId);

            return Result<ServiceDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service details for {ServiceId}", request.ServiceId);
            return Result<ServiceDetailDto>.Failure("An error occurred while retrieving the service details");
        }
    }
}
