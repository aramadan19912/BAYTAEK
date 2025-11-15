using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.Address;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Address;

public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, Result<List<AddressDto>>>
{
    private readonly IRepository<Domain.Entities.Address> _addressRepository;
    private readonly ILogger<GetUserAddressesQueryHandler> _logger;

    public GetUserAddressesQueryHandler(
        IRepository<Domain.Entities.Address> addressRepository,
        ILogger<GetUserAddressesQueryHandler> logger)
    {
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<Result<List<AddressDto>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all addresses for user
            var addresses = await _addressRepository.FindAsync(
                a => a.UserId == request.UserId,
                cancellationToken);

            var addressList = addresses?.OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ToList()
                ?? new List<Domain.Entities.Address>();

            // Map to DTOs
            var addressDtos = addressList.Select(a => new AddressDto
            {
                Id = a.Id,
                Label = a.Label,
                FullAddress = a.FullAddress,
                BuildingNumber = a.BuildingNumber,
                Street = a.Street,
                District = a.District,
                City = a.City,
                Region = a.Region.ToString(),
                PostalCode = a.PostalCode,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault,
                AdditionalInfo = a.AdditionalInfo,
                CreatedAt = a.CreatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} addresses for user {UserId}", addressDtos.Count, request.UserId);

            return Result<List<AddressDto>>.Success(addressDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving addresses for user {UserId}", request.UserId);
            return Result<List<AddressDto>>.Failure("An error occurred while retrieving addresses");
        }
    }
}
