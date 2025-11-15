using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Service : AuditableEntity
{
    public Guid CategoryId { get; set; }
    public Guid? ProviderId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public Currency Currency { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Region[] AvailableRegions { get; set; } = Array.Empty<Region>();
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
    public string? VideoUrl { get; set; }
    public string? RequiredMaterials { get; set; }
    public string? WarrantyInfo { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalBookings { get; set; }

    // Navigation properties
    public virtual ServiceCategory Category { get; set; } = null!;
    public virtual ServiceProvider? Provider { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();
}
