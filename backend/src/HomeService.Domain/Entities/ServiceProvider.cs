using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class ServiceProvider : AuditableEntity
{
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedBookings { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? CertificationDocuments { get; set; }
    public string? Portfolio { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<ProviderAvailability> Availabilities { get; set; } = new List<ProviderAvailability>();
}
