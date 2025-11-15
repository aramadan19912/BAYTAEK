using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public class UnregisterDeviceTokenCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class UnregisterDeviceTokenCommandHandler
    : IRequestHandler<UnregisterDeviceTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnregisterDeviceTokenCommandHandler> _logger;

    public UnregisterDeviceTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UnregisterDeviceTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UnregisterDeviceTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceToken = await _unitOfWork.Repository<DeviceToken>()
                .GetQueryable()
                .FirstOrDefaultAsync(dt => dt.Token == request.Token && dt.UserId == request.UserId,
                    cancellationToken);

            if (deviceToken == null)
                return Result.Failure<bool>("Device token not found");

            deviceToken.IsActive = false;
            _unitOfWork.Repository<DeviceToken>().Update(deviceToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Device token unregistered for user {UserId}", request.UserId);

            return Result.Success(true, "Device token unregistered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token for user {UserId}", request.UserId);
            return Result.Failure<bool>(
                "An error occurred while unregistering device token",
                ex.Message);
        }
    }
}
