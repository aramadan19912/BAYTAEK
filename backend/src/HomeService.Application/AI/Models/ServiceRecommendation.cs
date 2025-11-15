namespace HomeService.Application.AI.Models;

/// <summary>
/// Represents an AI-generated service recommendation
/// </summary>
public class ServiceRecommendation
{
    /// <summary>
    /// The service ID
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// The service name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// The service name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// The service description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// The service description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// The category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// The provider name
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// The base price
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// The currency
    /// </summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>
    /// The average rating (0-5)
    /// </summary>
    public double? AverageRating { get; set; }

    /// <summary>
    /// The total number of reviews
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// The total number of bookings
    /// </summary>
    public int TotalBookings { get; set; }

    /// <summary>
    /// The recommendation score (0-1)
    /// </summary>
    public double RecommendationScore { get; set; }

    /// <summary>
    /// The reason for this recommendation
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;

    /// <summary>
    /// Tags or features that match the user's preferences
    /// </summary>
    public List<string> MatchedFeatures { get; set; } = new();

    /// <summary>
    /// The provider's location/regions
    /// </summary>
    public List<string> AvailableRegions { get; set; } = new();

    /// <summary>
    /// Is this service currently available?
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }
}
