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

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<AddressCreatedDto>>
{
    private readonly IRepository<Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAddressCommandHandler> _logger;

    public CreateAddressCommandHandler(
        IRepository<Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAddressCommandHandler> logger)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AddressCreatedDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Label))
            {
                return Result<AddressCreatedDto>.Failure("Address label is required");
            }

            if (string.IsNullOrWhiteSpace(request.FullAddress))
            {
                return Result<AddressCreatedDto>.Failure("Full address is required");
            }

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return Result<AddressCreatedDto>.Failure("City is required");
            }

            // Validate coordinates
            if (request.Latitude < -90 || request.Latitude > 90)
            {
                return Result<AddressCreatedDto>.Failure("Invalid latitude");
            }

            if (request.Longitude < -180 || request.Longitude > 180)
            {
                return Result<AddressCreatedDto>.Failure("Invalid longitude");
            }

            // If this is set as default, unset other default addresses
            if (request.IsDefault)
            {
                var existingAddresses = await _addressRepository.FindAsync(
                    a => a.UserId == request.UserId && a.IsDefault,
                    cancellationToken);

                if (existingAddresses != null)
                {
                    foreach (var addr in existingAddresses)
                    {
                        addr.IsDefault = false;
                        await _addressRepository.UpdateAsync(addr, cancellationToken);
                    }
                }
            }

            // Create address
            var address = new Domain.Entities.Address
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Label = request.Label.Trim(),
                Street = request.Street?.Trim() ?? "",
                City = request.City.Trim(),
                State = request.District?.Trim() ?? "",
                PostalCode = request.PostalCode?.Trim() ?? "",
                BuildingNumber = request.BuildingNumber?.Trim(),
                Region = request.Region,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AdditionalDirections = request.AdditionalInfo?.Trim(),
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId.ToString()
            };

            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Address created for user {UserId}: {Label} in {City}",
                request.UserId, request.Label, request.City);

            var result = new AddressCreatedDto
            {
                Id = address.Id,
                Label = address.Label,
                FullAddress = address.FullAddress,
                IsDefault = address.IsDefault
            };

            return Result<AddressCreatedDto>.Success(result, "Address saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address for user {UserId}", request.UserId);
            return Result<AddressCreatedDto>.Failure("An error occurred while saving the address");
        }
    }
}
