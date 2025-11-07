using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? ProviderId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public decimal TotalAmount { get; set; }
    public Currency Currency { get; set; }
    public AddressDto Address { get; set; } = null!;
}

public class AddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public Region Region { get; set; }
}
