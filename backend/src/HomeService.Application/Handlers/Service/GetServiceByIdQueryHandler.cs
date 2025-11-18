using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Service;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Service;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDetailDto>>
{
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly ILogger<GetServiceByIdQueryHandler> _logger;

    public GetServiceByIdQueryHandler(
        IRepository<Domain.Entities.Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<ServiceCategory> categoryRepository,
        ILogger<GetServiceByIdQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _categoryRepository = categoryRepository;
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

            // Get provider info
            var provider = await _providerRepository.FindAsync(
                p => p.Id == service.ProviderId,
                cancellationToken);
            var providerEntity = provider?.FirstOrDefault();

            // Get provider user info
            Domain.Entities.User? providerUser = null;
            if (providerEntity != null)
            {
                providerUser = await _userRepository.GetByIdAsync(providerEntity.UserId, cancellationToken);
            }

            // Get reviews for this service (through bookings)
            var serviceBookings = await _bookingRepository.FindAsync(
                b => b.ServiceId == service.Id,
                cancellationToken);
            var bookingIds = serviceBookings?.Select(b => b.Id).ToList() ?? new List<Guid>();
            
            var allReviews = await _reviewRepository.FindAsync(
                r => bookingIds.Contains(r.BookingId) && r.IsVisible,
                cancellationToken);

            var reviewsList = allReviews?.OrderByDescending(r => r.CreatedAt).ToList() ?? new List<Domain.Entities.Review>();

            // Get category
            var category = await _categoryRepository.GetByIdAsync(service.CategoryId, cancellationToken);

            // Calculate average rating
            var averageRating = reviewsList.Any()
                ? (decimal)reviewsList.Average(r => r.Rating)
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

                ProviderId = service.ProviderId ?? Guid.Empty,
                ProviderName = providerUser != null
                    ? $"{providerUser.FirstName} {providerUser.LastName}"
                    : "Unknown Provider",
                ProviderBusinessName = providerEntity?.BusinessName,
                ProviderAverageRating = providerEntity?.AverageRating ?? 0,
                ProviderTotalReviews = providerEntity?.TotalReviews ?? 0,
                ProviderCompletedBookings = providerEntity?.CompletedBookings ?? 0,
                ProviderIsVerified = providerEntity?.IsVerified ?? false,

                BasePrice = service.BasePrice,
                Currency = service.Currency.ToString(),
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
