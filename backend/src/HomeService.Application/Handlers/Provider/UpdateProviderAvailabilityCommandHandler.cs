using HomeService.Application.Commands.Provider;
using HomeService.Domain.Interfaces;
using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Provider;

public class UpdateProviderAvailabilityCommandHandler : IRequestHandler<UpdateProviderAvailabilityCommand, Result<bool>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProviderAvailabilityCommandHandler> _logger;

    public UpdateProviderAvailabilityCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProviderAvailabilityCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateProviderAvailabilityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<bool>.Failure("Provider not found");
            }

            // Validate availability time range if provided
            if (request.AvailableFrom.HasValue && request.AvailableUntil.HasValue)
            {
                if (request.AvailableFrom.Value >= request.AvailableUntil.Value)
                {
                    return Result<bool>.Failure("Available from time must be before available until time");
                }

                // Check if the time range is in the past
                if (request.AvailableUntil.Value < DateTime.UtcNow)
                {
                    return Result<bool>.Failure("Availability time range cannot be in the past");
                }
            }

            // Track changes
            var hasChanges = false;

            // Update availability status
            if (provider.IsAvailable != request.IsAvailable)
            {
                provider.IsAvailable = request.IsAvailable;
                hasChanges = true;
            }

            // Update availability time range
            if (request.AvailableFrom.HasValue && provider.AvailableFrom != request.AvailableFrom.Value)
            {
                provider.AvailableFrom = request.AvailableFrom.Value;
                hasChanges = true;
            }

            if (request.AvailableUntil.HasValue && provider.AvailableUntil != request.AvailableUntil.Value)
            {
                provider.AvailableUntil = request.AvailableUntil.Value;
                hasChanges = true;
            }

            // If setting to unavailable, clear the time range
            if (!request.IsAvailable)
            {
                if (provider.AvailableFrom.HasValue || provider.AvailableUntil.HasValue)
                {
                    provider.AvailableFrom = null;
                    provider.AvailableUntil = null;
                    hasChanges = true;
                }
            }

            if (!hasChanges)
            {
                return Result<bool>.Success(true, "No changes detected");
            }

            // Update timestamps
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.ProviderId.ToString();

            await _providerRepository.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Availability updated for provider {ProviderId}. IsAvailable: {IsAvailable}, From: {From}, Until: {Until}",
                request.ProviderId, request.IsAvailable, request.AvailableFrom, request.AvailableUntil);

            var message = request.IsAvailable
                ? "Availability updated successfully. You are now available for bookings."
                : "You are now marked as unavailable for new bookings.";

            return Result<bool>.Success(true, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for provider {ProviderId}", request.ProviderId);
            return Result<bool>.Failure("An error occurred while updating availability");
        }
    }
}
