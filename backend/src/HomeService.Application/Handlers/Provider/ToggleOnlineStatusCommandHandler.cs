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

public class ToggleOnlineStatusCommandHandler : IRequestHandler<ToggleOnlineStatusCommand, Result<OnlineStatusDto>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleOnlineStatusCommandHandler> _logger;

    public ToggleOnlineStatusCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IUnitOfWork unitOfWork,
        ILogger<ToggleOnlineStatusCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<OnlineStatusDto>> Handle(ToggleOnlineStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<OnlineStatusDto>.Failure("Provider not found");
            }

            // Check if there's actually a change
            if (provider.IsOnline == request.IsOnline)
            {
                var statusDto = new OnlineStatusDto
                {
                    IsOnline = provider.IsOnline,
                    IsAvailable = provider.IsAvailable,
                    LastOnlineAt = provider.LastOnlineAt,
                    Message = provider.IsOnline ? "You are already online" : "You are already offline"
                };

                return Result<OnlineStatusDto>.Success(statusDto, "No change in online status");
            }

            // Update online status
            provider.IsOnline = request.IsOnline;

            // If going online, update last online timestamp
            if (request.IsOnline)
            {
                provider.LastOnlineAt = DateTime.UtcNow;
            }
            else
            {
                // If going offline, also mark as unavailable
                provider.IsAvailable = false;
                provider.AvailableFrom = null;
                provider.AvailableUntil = null;
            }

            // Update timestamps
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.ProviderId.ToString();

            await _providerRepository.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Provider {ProviderId} is now {Status}",
                request.ProviderId, request.IsOnline ? "online" : "offline");

            var responseDto = new OnlineStatusDto
            {
                IsOnline = provider.IsOnline,
                IsAvailable = provider.IsAvailable,
                LastOnlineAt = provider.LastOnlineAt,
                Message = provider.IsOnline
                    ? "You are now online and can receive booking requests"
                    : "You are now offline and will not receive new booking requests"
            };

            var message = provider.IsOnline
                ? "Status updated to online"
                : "Status updated to offline";

            return Result<OnlineStatusDto>.Success(responseDto, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling online status for provider {ProviderId}", request.ProviderId);
            return Result<OnlineStatusDto>.Failure("An error occurred while updating online status");
        }
    }
}
