using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.ProviderService;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.ProviderService;

public class GetProviderServicesQueryHandler : IRequestHandler<GetProviderServicesQuery, Result<List<ProviderServiceDto>>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ILogger<GetProviderServicesQueryHandler> _logger;

    public GetProviderServicesQueryHandler(
        IRepository<Service> serviceRepository,
        IRepository<Category> categoryRepository,
        IRepository<Booking> bookingRepository,
        IRepository<Review> reviewRepository,
        ILogger<GetProviderServicesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<List<ProviderServiceDto>>> Handle(GetProviderServicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider's services
            var servicesQuery = request.IsActive.HasValue
                ? await _serviceRepository.FindAsync(
                    s => s.ProviderId == request.ProviderId && s.IsActive == request.IsActive.Value,
                    cancellationToken)
                : await _serviceRepository.FindAsync(
                    s => s.ProviderId == request.ProviderId,
                    cancellationToken);

            var services = servicesQuery?.OrderByDescending(s => s.CreatedAt).ToList()
                ?? new List<Service>();

            if (!services.Any())
            {
                return Result<List<ProviderServiceDto>>.Success(new List<ProviderServiceDto>());
            }

            // Get categories
            var categoryIds = services.Select(s => s.CategoryId).Distinct().ToList();
            var categories = await _categoryRepository.FindAsync(
                c => categoryIds.Contains(c.Id),
                cancellationToken);
            var categoriesDict = categories?.ToDictionary(c => c.Id) ?? new Dictionary<Guid, Category>();

            // Get bookings for statistics
            var serviceIds = services.Select(s => s.Id).ToList();
            var bookings = await _bookingRepository.FindAsync(
                b => serviceIds.Contains(b.ServiceId),
                cancellationToken);
            var bookingsDict = bookings?.GroupBy(b => b.ServiceId)
                .ToDictionary(g => g.Key, g => g.ToList())
                ?? new Dictionary<Guid, List<Booking>>();

            // Get reviews
            var reviews = await _reviewRepository.FindAsync(
                r => serviceIds.Contains(r.ServiceId) && r.IsVisible,
                cancellationToken);
            var reviewsDict = reviews?.GroupBy(r => r.ServiceId)
                .ToDictionary(g => g.Key, g => g.ToList())
                ?? new Dictionary<Guid, List<Review>>();

            // Build DTOs
            var serviceDtos = services.Select(service =>
            {
                var category = categoriesDict.GetValueOrDefault(service.CategoryId);
                var serviceBookings = bookingsDict.GetValueOrDefault(service.Id, new List<Booking>());
                var serviceReviews = reviewsDict.GetValueOrDefault(service.Id, new List<Review>());

                return new ProviderServiceDto
                {
                    Id = service.Id,
                    CategoryId = service.CategoryId,
                    CategoryNameEn = category?.NameEn ?? "",
                    CategoryNameAr = category?.NameAr ?? "",
                    NameEn = service.NameEn,
                    NameAr = service.NameAr,
                    DescriptionEn = service.DescriptionEn,
                    DescriptionAr = service.DescriptionAr,
                    BasePrice = service.BasePrice,
                    Currency = service.Currency,
                    EstimatedDurationMinutes = service.EstimatedDurationMinutes,
                    AvailableRegions = service.AvailableRegions.Select(r => r.ToString()).ToList(),
                    RequiredMaterials = service.RequiredMaterials,
                    WarrantyInfo = service.WarrantyInfo,
                    ImageUrls = service.ImageUrls?.ToList() ?? new List<string>(),
                    VideoUrl = service.VideoUrl,
                    IsActive = service.IsActive,
                    IsFeatured = service.IsFeatured,
                    TotalBookings = serviceBookings.Count,
                    AverageRating = serviceReviews.Any()
                        ? Math.Round((decimal)serviceReviews.Average(r => r.Rating), 2)
                        : 0,
                    TotalReviews = serviceReviews.Count,
                    CreatedAt = service.CreatedAt,
                    UpdatedAt = service.UpdatedAt
                };
            }).ToList();

            _logger.LogInformation("Retrieved {Count} services for provider {ProviderId}",
                serviceDtos.Count, request.ProviderId);

            return Result<List<ProviderServiceDto>>.Success(serviceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services for provider {ProviderId}", request.ProviderId);
            return Result<List<ProviderServiceDto>>.Failure("An error occurred while retrieving services");
        }
    }
}
