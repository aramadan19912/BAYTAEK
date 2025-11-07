using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Provider;

public class SearchProvidersQueryHandler : IRequestHandler<SearchProvidersQuery, Result<ProviderSearchResultDto>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly ILogger<SearchProvidersQueryHandler> _logger;

    public SearchProvidersQueryHandler(
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.User> userRepository,
        ILogger<SearchProvidersQueryHandler> logger)
    {
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderSearchResultDto>> Handle(SearchProvidersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            // Get all providers (active only)
            var allProviders = await _providerRepository.FindAsync(
                p => true, // Get all, we'll filter below
                cancellationToken);

            var providers = allProviders?.ToList() ?? new List<ServiceProvider>();

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                providers = providers.Where(p =>
                    (p.BusinessName != null && p.BusinessName.ToLower().Contains(searchLower))
                ).ToList();
            }

            // Apply rating filter
            if (request.MinRating.HasValue)
            {
                providers = providers.Where(p => p.AverageRating >= request.MinRating.Value).ToList();
            }

            // Apply verification filter
            if (request.IsVerified.HasValue)
            {
                providers = providers.Where(p => p.IsVerified == request.IsVerified.Value).ToList();
            }

            // Apply online status filter
            if (request.IsOnline.HasValue)
            {
                providers = providers.Where(p => p.IsOnline == request.IsOnline.Value).ToList();
            }

            // Apply availability filter
            if (request.IsAvailable.HasValue)
            {
                providers = providers.Where(p => p.IsAvailable == request.IsAvailable.Value).ToList();
            }

            // Apply minimum completed bookings filter
            if (request.MinCompletedBookings.HasValue)
            {
                providers = providers.Where(p => p.CompletedBookings >= request.MinCompletedBookings.Value).ToList();
            }

            // TODO: Apply region filter when provider has region property
            // if (request.Region.HasValue)
            // {
            //     providers = providers.Where(p => p.Region == request.Region.Value).ToList();
            // }

            // TODO: Apply category filter when provider-service relationship is available
            // This would require joining with Service table
            // if (request.CategoryId.HasValue)
            // {
            //     providers = providers.Where(p => p.Services.Any(s => s.CategoryId == request.CategoryId.Value)).ToList();
            // }

            // Apply sorting
            providers = ApplySorting(providers, request.SortBy, request.SortOrder).ToList();

            // Get total count before pagination
            var totalCount = providers.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // Apply pagination
            var pagedProviders = providers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Get associated user data
            var userIds = pagedProviders.Select(p => p.UserId).Distinct().ToList();
            var users = await _userRepository.FindAsync(
                u => userIds.Contains(u.Id),
                cancellationToken);

            var usersDict = users?.ToDictionary(u => u.Id) ?? new Dictionary<Guid, Domain.Entities.User>();

            // Build result DTOs
            var providerDtos = pagedProviders.Select(provider =>
            {
                var user = usersDict.GetValueOrDefault(provider.UserId);

                return new ProviderSearchItemDto
                {
                    Id = provider.Id,
                    UserId = provider.UserId,
                    BusinessName = provider.BusinessName ?? "",
                    ProfileImageUrl = user?.ProfileImageUrl,
                    IsVerified = provider.IsVerified,
                    LicenseNumber = provider.IsVerified ? provider.LicenseNumber : null, // Only show if verified
                    AverageRating = provider.AverageRating,
                    TotalReviews = provider.TotalReviews,
                    CompletedBookings = provider.CompletedBookings,
                    IsOnline = provider.IsOnline,
                    IsAvailable = provider.IsAvailable,
                    AvailableFrom = provider.AvailableFrom,
                    AvailableUntil = provider.AvailableUntil,
                    Region = user?.Region.ToString() ?? "",
                    ServiceCategories = new List<string>(), // TODO: Populate when service relationship is available
                    TotalServices = 0, // TODO: Count when service relationship is available
                    LastOnlineAt = provider.LastOnlineAt,
                    CreatedAt = provider.CreatedAt
                };
            }).ToList();

            var result = new ProviderSearchResultDto
            {
                Providers = providerDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            _logger.LogInformation("Provider search returned {Count} out of {Total} providers. Page: {PageNumber}",
                providerDtos.Count, totalCount, request.PageNumber);

            return Result<ProviderSearchResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching providers");
            return Result<ProviderSearchResultDto>.Failure("An error occurred while searching providers");
        }
    }

    private IEnumerable<ServiceProvider> ApplySorting(IEnumerable<ServiceProvider> providers, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "rating" => isDescending
                ? providers.OrderByDescending(p => p.AverageRating)
                : providers.OrderBy(p => p.AverageRating),

            "reviews" => isDescending
                ? providers.OrderByDescending(p => p.TotalReviews)
                : providers.OrderBy(p => p.TotalReviews),

            "bookings" => isDescending
                ? providers.OrderByDescending(p => p.CompletedBookings)
                : providers.OrderBy(p => p.CompletedBookings),

            "newest" => isDescending
                ? providers.OrderByDescending(p => p.CreatedAt)
                : providers.OrderBy(p => p.CreatedAt),

            "name" => isDescending
                ? providers.OrderByDescending(p => p.BusinessName)
                : providers.OrderBy(p => p.BusinessName),

            // Default: sort by rating (best first), then by total reviews
            _ => providers.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.TotalReviews)
        };
    }
}
