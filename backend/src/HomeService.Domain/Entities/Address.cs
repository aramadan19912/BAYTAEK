using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Address : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty; // Home, Work, etc.
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public Region Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? AdditionalDirections { get; set; }
    public bool IsDefault { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
