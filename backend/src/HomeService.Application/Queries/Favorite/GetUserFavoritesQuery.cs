using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Favorite;

public class GetUserFavoritesQuery : IRequest<Result<List<FavoriteServiceDto>>>
{
    public Guid UserId { get; set; }
}

public class FavoriteServiceDto
{
    public Guid FavoriteId { get; set; }
    public Guid ServiceId { get; set; }

    // Service details
    public string ServiceNameEn { get; set; } = string.Empty;
    public string ServiceNameAr { get; set; } = string.Empty;
    public string? ServiceDescriptionEn { get; set; }
    public string? ServiceDescriptionAr { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? ImageUrl { get; set; }

    // Category
    public string CategoryNameEn { get; set; } = string.Empty;
    public string CategoryNameAr { get; set; } = string.Empty;

    // Provider
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal ProviderRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsProviderVerified { get; set; }

    // Service status
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }

    // Favorite metadata
    public DateTime AddedAt { get; set; }
}
