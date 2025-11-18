using HomeService.Application.Commands.ProviderVerification;
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

namespace HomeService.Application.Handlers.ProviderVerification;

public class ApproveVerificationCommandHandler : IRequestHandler<ApproveVerificationCommand, Result<bool>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<ApproveVerificationCommandHandler> _logger;

    public ApproveVerificationCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        INotificationService notificationService,
        IPushNotificationService pushNotificationService,
        ILogger<ApproveVerificationCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _notificationService = notificationService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ApproveVerificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<bool>.Failure("Provider not found");
            }

            // Check if already verified
            if (provider.IsVerified)
            {
                return Result<bool>.Failure("Provider is already verified");
            }

            // Approve verification
            provider.IsVerified = true;
            // Note: VerificationStatus, VerifiedAt, VerifiedBy would need to be added to ServiceProvider entity
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.AdminUserId.ToString();

            await _providerRepository.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Provider {ProviderId} verified by admin {AdminId}",
                request.ProviderId, request.AdminUserId);

            // Get user for notifications
            var user = await _userRepository.GetByIdAsync(provider.UserId, cancellationToken);

            // Send email notification
            if (user != null)
            {
                try
                {
                    await _emailService.SendProviderApprovalEmailAsync(
                        user.Email,
                        provider.BusinessName ?? user.FirstName,
                        true,
                        null,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification approval email to provider {ProviderId}",
                        request.ProviderId);
                }

                // Send notification
                try
                {
                    await _notificationService.SendProviderApprovalNotificationAsync(
                        provider.UserId,
                        true,
                        null,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification to provider {ProviderId}",
                        request.ProviderId);
                }
            }

            return Result<bool>.Success(true, "Provider verification approved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving verification for provider {ProviderId}", request.ProviderId);
            return Result<bool>.Failure("An error occurred while approving verification");
        }
    }
}
