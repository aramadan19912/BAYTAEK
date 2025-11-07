using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class Review : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProviderId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
    public string[] VideoUrls { get; set; } = Array.Empty<string>();
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderRespondedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsVisible { get; set; }
    public decimal? SentimentScore { get; set; } // AI sentiment analysis score

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual User Customer { get; set; } = null!;
    public virtual ServiceProvider Provider { get; set; } = null!;
}
