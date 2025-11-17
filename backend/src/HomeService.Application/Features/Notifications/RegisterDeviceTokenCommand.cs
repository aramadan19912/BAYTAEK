using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public class RegisterDeviceTokenCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
}

public class RegisterDeviceTokenCommandHandler
    : IRequestHandler<RegisterDeviceTokenCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterDeviceTokenCommandHandler> _logger;

    public RegisterDeviceTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RegisterDeviceTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RegisterDeviceTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if token already exists
            var existingToken = await _unitOfWork.Repository<DeviceToken>()
                .GetQueryable()
                .FirstOrDefaultAsync(dt => dt.Token == request.Token, cancellationToken);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.UserId = request.UserId;
                existingToken.DeviceType = request.DeviceType;
                existingToken.DeviceId = request.DeviceId;
                existingToken.IsActive = true;
                existingToken.LastUsedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeviceToken>().Update(existingToken);
            }
            else
            {
                // Create new token
                var deviceToken = new DeviceToken
                {
                    UserId = request.UserId,
                    Token = request.Token,
                    DeviceType = request.DeviceType,
                    DeviceId = request.DeviceId,
                    IsActive = true,
                    LastUsedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<DeviceToken>().AddAsync(deviceToken, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Device token registered for user {UserId}", request.UserId);

            return Result.Success(true, "Device token registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", request.UserId);
            return Result.Failure<bool>(
                "An error occurred while registering device token",
                ex.Message);
        }
    }
}
