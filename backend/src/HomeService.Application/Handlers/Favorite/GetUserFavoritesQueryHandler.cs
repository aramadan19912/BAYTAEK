using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Favorite;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Favorite;

public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, Result<List<FavoriteServiceDto>>>
{
    // TODO: Uncomment when Favorite entity is created in Domain layer
    // private readonly IRepository<Domain.Entities.Favorite> _favoriteRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<Domain.Entities.ServiceProvider> _providerRepository;
    private readonly ILogger<GetUserFavoritesQueryHandler> _logger;

    public GetUserFavoritesQueryHandler(
        // IRepository<Domain.Entities.Favorite> favoriteRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        IRepository<Domain.Entities.ServiceProvider> providerRepository,
        ILogger<GetUserFavoritesQueryHandler> logger)
    {
        // _favoriteRepository = favoriteRepository;
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<Result<List<FavoriteServiceDto>>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when Favorite entity exists
            /*
            // Get user favorites
            var favorites = await _favoriteRepository.FindAsync(
                f => f.UserId == request.UserId,
                cancellationToken);

            var favoritesList = favorites?.OrderByDescending(f => f.CreatedAt).ToList()
                ?? new List<Domain.Entities.Favorite>();

            // Get service IDs
            var serviceIds = favoritesList.Select(f => f.ServiceId).ToList();

            // Get services
            var services = await _serviceRepository.FindAsync(
                s => serviceIds.Contains(s.Id),
                cancellationToken);

            var servicesList = services?.ToList() ?? new List<Domain.Entities.Service>();

            // Get providers and categories
            var providerIds = servicesList.Select(s => s.ProviderId).Distinct().ToList();
            var categoryIds = servicesList.Select(s => s.CategoryId).Distinct().ToList();

            var providers = await _providerRepository.FindAsync(
                p => providerIds.Contains(p.Id),
                cancellationToken);

                c => categoryIds.Contains(c.Id),
                cancellationToken);

            var providersDict = providers?.ToDictionary(p => p.Id) ?? new Dictionary<Guid, Domain.Entities.ServiceProvider>();

            // Build DTOs
            var favoriteDtos = favoritesList.Select(fav =>
            {
                var service = servicesList.FirstOrDefault(s => s.Id == fav.ServiceId);
                if (service == null) return null;

                var provider = providersDict.GetValueOrDefault(service.ProviderId);
                var category = categoriesDict.GetValueOrDefault(service.CategoryId);

                return new FavoriteServiceDto
                {
                    FavoriteId = fav.Id,
                    ServiceId = service.Id,
                    ServiceNameEn = service.NameEn,
                    ServiceNameAr = service.NameAr,
                    ServiceDescriptionEn = service.DescriptionEn,
                    ServiceDescriptionAr = service.DescriptionAr,
                    BasePrice = service.BasePrice,
                    Currency = service.Currency,
                    ImageUrl = service.ImageUrls?.FirstOrDefault(),
                    CategoryNameEn = category?.NameEn ?? "",
                    CategoryNameAr = category?.NameAr ?? "",
                    ProviderId = service.ProviderId,
                    ProviderName = provider?.BusinessName ?? "",
                    ProviderRating = provider?.AverageRating ?? 0,
                    TotalReviews = provider?.TotalReviews ?? 0,
                    IsProviderVerified = provider?.IsVerified ?? false,
                    IsActive = service.IsActive,
                    IsFeatured = service.IsFeatured,
                    AddedAt = fav.CreatedAt
                };
            }).Where(f => f != null).ToList()!;

            _logger.LogInformation("Retrieved {Count} favorites for user {UserId}", favoriteDtos.Count, request.UserId);

            return Result<List<FavoriteServiceDto>>.Success(favoriteDtos);
            */

            // Temporary: Return empty result until Favorite entity is created
            _logger.LogWarning("Favorite entity not yet implemented. Returning empty result for user {UserId}", request.UserId);

            return Result<List<FavoriteServiceDto>>.Success(
                new List<FavoriteServiceDto>(),
                "Favorites system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorites for user {UserId}", request.UserId);
            return Result<List<FavoriteServiceDto>>.Failure("An error occurred while retrieving favorites");
        }
    }
}
