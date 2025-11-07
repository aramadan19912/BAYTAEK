using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Services;

public record GetServicesQuery : IRequest<Result<PagedResult<ServiceDto>>>
{
    public Guid? CategoryId { get; init; }
    public Region? Region { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsFeatured { get; init; }

    // Price filters
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }

    // Rating filter
    public decimal? MinRating { get; init; } // Filter by minimum average rating

    // Provider filter
    public Guid? ProviderId { get; init; } // Get services by specific provider
    public bool? VerifiedProvidersOnly { get; init; } // Only verified providers

    // Sorting
    public string? SortBy { get; init; } // "price", "rating", "newest", "popular"
    public string? SortOrder { get; init; } // "asc", "desc"

    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
