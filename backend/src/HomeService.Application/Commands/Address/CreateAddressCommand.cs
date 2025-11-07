using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Address;

public class CreateAddressCommand : IRequest<Result<AddressCreatedDto>>
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty; // "Home", "Work", "Other"
    public string FullAddress { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string City { get; set; } = string.Empty;
    public Region Region { get; set; }
    public string? PostalCode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class AddressCreatedDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
