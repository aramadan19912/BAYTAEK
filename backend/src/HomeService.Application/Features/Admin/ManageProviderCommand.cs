using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Admin;

public record ManageProviderCommand(
    Guid ProviderId,
    Guid AdminUserId,
    ProviderManagementAction Action,
    string? Reason = null
) : IRequest<Result<ProviderManagementDto>>;

public enum ProviderManagementAction
{
    Verify,
    Unverify,
    Suspend,
    Activate
}

public class ManageProviderCommandHandler
    : IRequestHandler<ManageProviderCommand, Result<ProviderManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ManageProviderCommandHandler> _logger;

    public ManageProviderCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ManageProviderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<ProviderManagementDto>> Handle(
        ManageProviderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetQueryable()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ProviderManagementDto>("Provider not found");

            string actionMessage;
            string notificationTitle;
            string notificationMessage;

            switch (request.Action)
            {
                case ProviderManagementAction.Verify:
                    provider.IsVerified = true;
                    actionMessage = "Provider verified successfully";
                    notificationTitle = "Provider Verification Approved";
                    notificationMessage = "Congratulations! Your provider account has been verified. " +
                                        "You can now offer services on the platform.";
                    break;

                case ProviderManagementAction.Unverify:
                    provider.IsVerified = false;
                    actionMessage = "Provider unverified successfully";
                    notificationTitle = "Provider Verification Removed";
                    notificationMessage = !string.IsNullOrEmpty(request.Reason)
                        ? $"Your provider verification has been removed. Reason: {request.Reason}"
                        : "Your provider verification has been removed. Please resubmit your documents.";
                    break;

                case ProviderManagementAction.Suspend:
                    provider.IsActive = false;
                    actionMessage = "Provider suspended successfully";
                    notificationTitle = "Provider Account Suspended";
                    notificationMessage = !string.IsNullOrEmpty(request.Reason)
                        ? $"Your provider account has been suspended. Reason: {request.Reason}"
                        : "Your provider account has been suspended. Contact support for assistance.";
                    break;

                case ProviderManagementAction.Activate:
                    provider.IsActive = true;
                    actionMessage = "Provider activated successfully";
                    notificationTitle = "Provider Account Activated";
                    notificationMessage = "Your provider account has been activated. You can resume offering services.";
                    break;

                default:
                    return Result.Failure<ProviderManagementDto>("Invalid action");
            }

            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.AdminUserId.ToString();

            _unitOfWork.Repository<ServiceProvider>().Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to provider
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        provider.UserId,
                        notificationTitle,
                        notificationTitle,
                        notificationMessage,
                        notificationMessage,
                        NotificationCategory.Account,
                        provider.Id,
                        nameof(ServiceProvider),
                        "/provider/profile",
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending provider management notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Admin {AdminId} performed {Action} on provider {ProviderId}. Reason: {Reason}",
                request.AdminUserId, request.Action, request.ProviderId, request.Reason ?? "None");

            var dto = new ProviderManagementDto
            {
                ProviderId = provider.Id,
                BusinessName = provider.BusinessName,
                Email = provider.User.Email,
                IsVerified = provider.IsVerified,
                IsActive = provider.IsActive,
                Action = request.Action.ToString(),
                ActionDate = DateTime.UtcNow
            };

            return Result.Success(dto, actionMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderManagementDto>(
                "An error occurred while managing the provider",
                ex.Message);
        }
    }
}

public class ProviderManagementDto
{
    public Guid ProviderId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
}
