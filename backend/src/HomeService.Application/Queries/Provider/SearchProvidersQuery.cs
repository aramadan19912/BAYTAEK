using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public class SearchProvidersQuery : IRequest<Result<ProviderSearchResultDto>>
{
    // Basic filters
    public string? SearchTerm { get; set; } // Search in business name
    public Region? Region { get; set; }
    public Guid? CategoryId { get; set; } // Providers offering services in this category

    // Rating and verification
    public decimal? MinRating { get; set; } // Minimum average rating
    public bool? IsVerified { get; set; } // Only verified providers

    // Availability
    public bool? IsOnline { get; set; } // Currently online
    public bool? IsAvailable { get; set; } // Currently available for bookings

    // Performance filters
    public int? MinCompletedBookings { get; set; } // Providers with X+ completed jobs

    // Sorting
    public string? SortBy { get; set; } // "rating", "reviews", "bookings", "newest"
    public string? SortOrder { get; set; } // "asc", "desc"

    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProviderSearchResultDto
{
    public List<ProviderSearchItemDto> Providers { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProviderSearchItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Business info
    public string BusinessName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsVerified { get; set; }
    public string? LicenseNumber { get; set; }

    // Performance metrics
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedBookings { get; set; }

    // Availability
    public bool IsOnline { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }

    // Location
    public string Region { get; set; } = string.Empty;

    // Service categories (summary)
    public List<string> ServiceCategories { get; set; } = new();
    public int TotalServices { get; set; }

    // Additional info
    public DateTime? LastOnlineAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
