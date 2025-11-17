using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Service;

public class GetServiceByIdQuery : IRequest<Result<ServiceDetailDto>>
{
    public Guid ServiceId { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
}

public class ServiceDetailDto
{
    public Guid Id { get; set; }

    // Multi-language fields
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    // Category information
    public Guid CategoryId { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;

    // Provider information
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderBusinessName { get; set; }
    public decimal ProviderAverageRating { get; set; }
    public int ProviderTotalReviews { get; set; }
    public int ProviderCompletedBookings { get; set; }
    public bool ProviderIsVerified { get; set; }

    // Service details
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "SAR";
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }

    // Media
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }

    // Regional availability
    public List<string> AvailableRegions { get; set; } = new();

    // Additional information
    public string? RequiredMaterials { get; set; }
    public string? WarrantyInfo { get; set; }

    // Reviews summary
    public List<ServiceReviewDto> RecentReviews { get; set; } = new();
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ServiceReviewDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? ProviderResponse { get; set; }
}
