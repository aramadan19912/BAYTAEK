using AutoMapper;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Application.Queries.Services;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Services;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, Result<PagedResult<ServiceDto>>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetServicesQueryHandler> _logger;

    public GetServicesQueryHandler(
        IRepository<Service> serviceRepository,
        IMapper mapper,
        ILogger<GetServicesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Build query - start with active services
            var query = await _serviceRepository.FindAsync(s => s.IsActive, cancellationToken);

            // Apply basic filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == request.CategoryId.Value);
            }

            if (request.Region.HasValue)
            {
                query = query.Where(s => s.AvailableRegions.Contains(request.Region.Value));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.NameEn.ToLower().Contains(searchLower) ||
                    s.NameAr.Contains(searchLower) ||
                    s.DescriptionEn.ToLower().Contains(searchLower) ||
                    s.DescriptionAr.Contains(searchLower));
            }

            if (request.IsFeatured.HasValue)
            {
                query = query.Where(s => s.IsFeatured == request.IsFeatured.Value);
            }

            // Price filters
            if (request.MinPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice <= request.MaxPrice.Value);
            }

            // Provider filters
            if (request.ProviderId.HasValue)
            {
                query = query.Where(s => s.ProviderId == request.ProviderId.Value);
            }

            // TODO: Implement VerifiedProvidersOnly when provider relationship is available
            // This would require joining with ServiceProvider table
            // if (request.VerifiedProvidersOnly.HasValue && request.VerifiedProvidersOnly.Value)
            // {
            //     query = query.Where(s => s.Provider.IsVerified);
            // }

            // TODO: Implement MinRating filter when service ratings are available
            // This would require calculating average rating from reviews
            // if (request.MinRating.HasValue)
            // {
            //     query = query.Where(s => s.AverageRating >= request.MinRating.Value);
            // }

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortOrder);

            // Get total count before pagination
            var totalCount = query.Count();

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var serviceDtos = _mapper.Map<List<ServiceDto>>(items);

            var pagedResult = new PagedResult<ServiceDto>
            {
                Items = serviceDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            _logger.LogInformation("Retrieved {Count} services out of {Total}. Filters: Category={Category}, Region={Region}, Search={Search}",
                items.Count, totalCount, request.CategoryId, request.Region, request.SearchTerm);

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services");
            return Result.Failure<PagedResult<ServiceDto>>("An error occurred while retrieving services", ex.Message);
        }
    }

    private IEnumerable<Service> ApplySorting(IEnumerable<Service> query, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "price" => isDescending
                ? query.OrderByDescending(s => s.BasePrice)
                : query.OrderBy(s => s.BasePrice),

            "newest" => isDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),

            "name" => isDescending
                ? query.OrderByDescending(s => s.NameEn)
                : query.OrderBy(s => s.NameEn),

            // TODO: Implement rating sort when service ratings are available
            // "rating" => isDescending
            //     ? query.OrderByDescending(s => s.AverageRating)
            //     : query.OrderBy(s => s.AverageRating),

            // TODO: Implement popular sort based on booking count
            // "popular" => isDescending
            //     ? query.OrderByDescending(s => s.TotalBookings)
            //     : query.OrderBy(s => s.TotalBookings),

            // Default: sort by featured first, then by creation date
            _ => query.OrderByDescending(s => s.IsFeatured).ThenByDescending(s => s.CreatedAt)
        };
    }
}
