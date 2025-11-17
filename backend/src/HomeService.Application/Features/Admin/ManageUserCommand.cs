using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Admin;

public record ManageUserCommand(
    Guid UserId,
    Guid AdminUserId,
    UserManagementAction Action,
    string? Reason = null
) : IRequest<Result<UserManagementDto>>;

public enum UserManagementAction
{
    Suspend,
    Activate,
    Verify,
    Unverify,
    Delete
}

public class ManageUserCommandHandler : IRequestHandler<ManageUserCommand, Result<UserManagementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ManageUserCommandHandler> _logger;

    public ManageUserCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ManageUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<UserManagementDto>> Handle(
        ManageUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>()
                .GetByIdAsync(request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<UserManagementDto>("User not found");

            string actionMessage;
            string notificationTitle;
            string notificationMessage;

            switch (request.Action)
            {
                case UserManagementAction.Suspend:
                    user.IsActive = false;
                    actionMessage = "User suspended successfully";
                    notificationTitle = "Account Suspended";
                    notificationMessage = !string.IsNullOrEmpty(request.Reason)
                        ? $"Your account has been suspended. Reason: {request.Reason}"
                        : "Your account has been suspended. Please contact support for more information.";
                    break;

                case UserManagementAction.Activate:
                    user.IsActive = true;
                    actionMessage = "User activated successfully";
                    notificationTitle = "Account Activated";
                    notificationMessage = "Your account has been activated. You can now access all features.";
                    break;

                case UserManagementAction.Verify:
                    user.EmailVerified = true;
                    user.PhoneVerified = true;
                    actionMessage = "User verified successfully";
                    notificationTitle = "Account Verified";
                    notificationMessage = "Your account has been verified. Enjoy full platform access!";
                    break;

                case UserManagementAction.Unverify:
                    user.EmailVerified = false;
                    user.PhoneVerified = false;
                    actionMessage = "User unverified successfully";
                    notificationTitle = "Verification Removed";
                    notificationMessage = !string.IsNullOrEmpty(request.Reason)
                        ? $"Your account verification has been removed. Reason: {request.Reason}"
                        : "Your account verification has been removed. Please verify your account again.";
                    break;

                case UserManagementAction.Delete:
                    // Soft delete - mark as inactive and anonymize data
                    user.IsActive = false;
                    user.Email = $"deleted_{user.Id}@deleted.com";
                    user.PhoneNumber = null;
                    actionMessage = "User deleted successfully";
                    notificationTitle = "Account Deleted";
                    notificationMessage = "Your account has been deleted as per your request or platform policy.";
                    break;

                default:
                    return Result.Failure<UserManagementDto>("Invalid action");
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = request.AdminUserId.ToString();

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to user
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        request.UserId,
                        notificationTitle,
                        notificationTitle,
                        notificationMessage,
                        notificationMessage,
                        NotificationCategory.Account,
                        user.Id,
                        nameof(User),
                        "/profile",
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending user management notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Admin {AdminId} performed {Action} on user {UserId}. Reason: {Reason}",
                request.AdminUserId, request.Action, request.UserId, request.Reason ?? "None");

            var dto = new UserManagementDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                IsActive = user.IsActive,
                EmailVerified = user.EmailVerified,
                PhoneVerified = user.PhoneVerified,
                Action = request.Action.ToString(),
                ActionDate = DateTime.UtcNow
            };

            return Result.Success(dto, actionMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing user {UserId}", request.UserId);
            return Result.Failure<UserManagementDto>(
                "An error occurred while managing the user",
                ex.Message);
        }
    }
}

public class UserManagementDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
}
