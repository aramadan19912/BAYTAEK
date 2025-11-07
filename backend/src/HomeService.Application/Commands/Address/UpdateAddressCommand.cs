using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Address;

public class UpdateAddressCommand : IRequest<Result<bool>>
{
    public Guid AddressId { get; set; }
    public Guid UserId { get; set; }
    public string? Label { get; set; }
    public string? FullAddress { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public Region? Region { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool? IsDefault { get; set; }
    public string? AdditionalInfo { get; set; }
}
