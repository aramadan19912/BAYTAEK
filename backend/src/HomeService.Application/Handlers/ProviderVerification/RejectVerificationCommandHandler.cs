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

public class RejectVerificationCommandHandler : IRequestHandler<RejectVerificationCommand, Result<bool>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<RejectVerificationCommandHandler> _logger;

    public RejectVerificationCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        ILogger<RejectVerificationCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RejectVerificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<bool>.Failure("Provider not found");
            }

            // Validate reason
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result<bool>.Failure("Rejection reason is required");
            }

            // Reject verification
            provider.IsVerified = false;
            provider.VerificationStatus = "Rejected";
            provider.VerificationRejectionReason = request.Reason.Trim();
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.AdminUserId.ToString();

            await _providerRepository.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Provider {ProviderId} verification rejected by admin {AdminId}. Reason: {Reason}",
                request.ProviderId, request.AdminUserId, request.Reason);

            // Get user for notifications
            var user = await _userRepository.GetByIdAsync(provider.UserId, cancellationToken);

            // Send email notification
            if (user != null)
            {
                try
                {
                    await _emailService.SendProviderVerificationRejectedEmailAsync(
                        user.Email,
                        provider.BusinessName ?? user.FirstName,
                        request.Reason,
                        user.PreferredLanguage,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification rejection email to provider {ProviderId}",
                        request.ProviderId);
                }

                // Send push notification
                try
                {
                    await _pushNotificationService.SendNotificationAsync(
                        provider.UserId,
                        "Verification Not Approved",
                        $"Your verification request was not approved. Reason: {request.Reason}",
                        new Dictionary<string, string>
                        {
                            { "type", "verification_rejected" },
                            { "providerId", provider.Id.ToString() },
                            { "reason", request.Reason }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification to provider {ProviderId}",
                        request.ProviderId);
                }
            }

            return Result<bool>.Success(true, "Provider verification rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting verification for provider {ProviderId}", request.ProviderId);
            return Result<bool>.Failure("An error occurred while rejecting verification");
        }
    }
}
