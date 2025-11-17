using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Review;

public class SubmitReviewCommand : IRequest<Result<ReviewDto>>
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string? Comment { get; set; }
    public List<string>? ImageUrls { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }

    // Customer info (or anonymous)
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerProfileImage { get; set; }

    // Review content
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsAnonymous { get; set; }

    // Provider response
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderResponseAt { get; set; }

    // Metadata
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
