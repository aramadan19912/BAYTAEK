using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Review;

public class GetServiceReviewsQuery : IRequest<Result<ServiceReviewsDto>>
{
    public Guid ServiceId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MinRating { get; set; } // Filter by minimum rating (1-5)
    public bool WithImagesOnly { get; set; } = false; // Only show reviews with images
}

public class ServiceReviewsDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public ReviewsSummaryDto Summary { get; set; } = new();
    public List<ReviewDetailDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ReviewsSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Star -> Count
    public int ReviewsWithImages { get; set; }
    public int ReviewsWithComments { get; set; }
}

public class ReviewDetailDto
{
    public Guid Id { get; set; }

    // Customer info
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerProfileImage { get; set; }

    // Provider info (for response)
    public string ProviderName { get; set; } = string.Empty;

    // Review content
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsAnonymous { get; set; }

    // Provider response
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderResponseAt { get; set; }

    // Service info (optional, for display)
    public string? ServiceName { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public bool IsHelpful { get; set; } // Can be used for "Was this review helpful?"
    public int HelpfulCount { get; set; } // Count of users who found it helpful
}
