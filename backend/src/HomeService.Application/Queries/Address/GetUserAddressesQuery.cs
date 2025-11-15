using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.Address;

public class GetUserAddressesQuery : IRequest<Result<List<AddressDto>>>
{
    public Guid UserId { get; set; }
}

public class AddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty; // "Home", "Work", "Other"
    public string FullAddress { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime CreatedAt { get; set; }
}
