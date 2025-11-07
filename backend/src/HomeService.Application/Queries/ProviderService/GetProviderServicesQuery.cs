using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.ProviderService;

public class GetProviderServicesQuery : IRequest<Result<List<ProviderServiceDto>>>
{
    public Guid ProviderId { get; set; }
    public bool? IsActive { get; set; } // Filter by active status
}

public class ProviderServiceDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;

    // Service details
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    // Pricing
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "SAR";

    // Service info
    public int EstimatedDurationMinutes { get; set; }
    public List<string> AvailableRegions { get; set; } = new();

    // Additional info
    public string? RequiredMaterials { get; set; }
    public string? WarrantyInfo { get; set; }

    // Media
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }

    // Statistics
    public int TotalBookings { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Dates
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
