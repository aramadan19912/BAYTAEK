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

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAddressCommandHandler> _logger;

    public DeleteAddressCommandHandler(
        IRepository<Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAddressCommandHandler> logger)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("User {UserId} attempted to delete address {AddressId} they don't own",
                    request.UserId, request.AddressId);
                return Result<bool>.Failure("You are not authorized to delete this address");
            }

            // Delete address
            await _addressRepository.DeleteAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Address {AddressId} ({Label}) deleted by user {UserId}",
                request.AddressId, address.Label, request.UserId);

            return Result<bool>.Success(true, "Address deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", request.AddressId);
            return Result<bool>.Failure("An error occurred while deleting the address");
        }
    }
}
