using HomeService.Application.Commands.Address;
using HomeService.Domain.Interfaces;
using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Address;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAddressCommandHandler> _logger;

    public UpdateAddressCommandHandler(
        IRepository<Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAddressCommandHandler> logger)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get address
            var address = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
            if (address == null)
            {
                return Result<bool>.Failure("Address not found");
            }

            // Verify user owns this address
            if (address.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to update address {AddressId} they don't own",
                    request.UserId, request.AddressId);
                return Result<bool>.Failure("You are not authorized to update this address");
            }

            // Track changes
            var hasChanges = false;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Label) && request.Label != address.Label)
            {
                address.Label = request.Label.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.FullAddress) && request.FullAddress != address.FullAddress)
            {
                hasChanges = true;
            }

            if (request.BuildingNumber != null && request.BuildingNumber != address.BuildingNumber)
            {
                address.BuildingNumber = string.IsNullOrWhiteSpace(request.BuildingNumber) ? null : request.BuildingNumber.Trim();
                hasChanges = true;
            }

            if (request.Street != null && request.Street != address.Street)
            {
                address.Street = string.IsNullOrWhiteSpace(request.Street) ? null : request.Street.Trim();
                hasChanges = true;
            }

            if (request.District != null && request.District != address.District)
            {
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.City) && request.City != address.City)
            {
                address.City = request.City.Trim();
                hasChanges = true;
            }

            if (request.Region.HasValue && request.Region.Value != address.Region)
            {
                address.Region = request.Region.Value;
                hasChanges = true;
            }

            if (request.PostalCode != null && request.PostalCode != address.PostalCode)
            {
                address.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
                hasChanges = true;
            }

            if (request.Latitude.HasValue && request.Latitude.Value != address.Latitude)
            {
                // Validate coordinates
                if (request.Latitude.Value < -90 || request.Latitude.Value > 90)
                {
                    return Result<bool>.Failure("Invalid latitude");
                }
                address.Latitude = request.Latitude.Value;
                hasChanges = true;
            }

            if (request.Longitude.HasValue && request.Longitude.Value != address.Longitude)
            {
                // Validate coordinates
                if (request.Longitude.Value < -180 || request.Longitude.Value > 180)
                {
                    return Result<bool>.Failure("Invalid longitude");
                }
                address.Longitude = request.Longitude.Value;
                hasChanges = true;
            }

            if (request.AdditionalInfo != null && request.AdditionalInfo != address.AdditionalInfo)
            {
                hasChanges = true;
            }

            // Handle default address change
            if (request.IsDefault.HasValue && request.IsDefault.Value != address.IsDefault)
            {
                if (request.IsDefault.Value)
                {
                    // Unset other default addresses
                    var otherAddresses = await _addressRepository.FindAsync(
                        a => a.UserId == request.UserId && a.IsDefault && a.Id != request.AddressId,
                        cancellationToken);

                    if (otherAddresses != null)
                    {
                        foreach (var addr in otherAddresses)
                        {
                            addr.IsDefault = false;
                            await _addressRepository.UpdateAsync(addr, cancellationToken);
                        }
                    }
                }

                address.IsDefault = request.IsDefault.Value;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return Result<bool>.Success(true, "No changes detected");
            }

            // Update audit fields
            address.UpdatedAt = DateTime.UtcNow;
            address.UpdatedBy = request.UserId.ToString();

            await _addressRepository.UpdateAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Address {AddressId} updated by user {UserId}", request.AddressId, request.UserId);

            return Result<bool>.Success(true, "Address updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", request.AddressId);
            return Result<bool>.Failure("An error occurred while updating the address");
        }
    }
}
