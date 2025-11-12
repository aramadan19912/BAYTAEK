using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Admin;

public record ManageDisputeCommand(
    Guid DisputeId,
    Guid AdminUserId,
    DisputeManagementAction Action,
    DisputeStatus? NewStatus = null,
    DisputePriority? NewPriority = null,
    Guid? AssignTo = null,
    string? Resolution = null
) : IRequest<Result<DisputeDto>>;

public enum DisputeManagementAction
{
    Assign,
    UpdateStatus,
    UpdatePriority,
    Resolve,
    Close,
    Escalate
}

public class ManageDisputeCommandHandler
    : IRequestHandler<ManageDisputeCommand, Result<DisputeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ManageDisputeCommandHandler> _logger;

    public ManageDisputeCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ManageDisputeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<DisputeDto>> Handle(
        ManageDisputeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dispute = await _unitOfWork.Repository<Dispute>()
                .GetQueryable()
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Service)
                .Include(d => d.RaisedByUser)
                .Include(d => d.AssignedToUser)
                .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

            if (dispute == null)
                return Result.Failure<DisputeDto>("Dispute not found");

            string actionMessage;
            string notificationTitle = string.Empty;
            string notificationMessage = string.Empty;
            bool sendNotification = false;

            switch (request.Action)
            {
                case DisputeManagementAction.Assign:
                    if (!request.AssignTo.HasValue)
                        return Result.Failure<DisputeDto>("AssignTo is required for assign action");

                    dispute.AssignedTo = request.AssignTo.Value;
                    dispute.Status = DisputeStatus.UnderReview;
                    actionMessage = "Dispute assigned successfully";
                    sendNotification = true;
                    notificationTitle = "Dispute Update";
                    notificationMessage = "Your dispute is now under review by our support team.";
                    break;

                case DisputeManagementAction.UpdateStatus:
                    if (!request.NewStatus.HasValue)
                        return Result.Failure<DisputeDto>("NewStatus is required for update status action");

                    dispute.Status = request.NewStatus.Value;
                    actionMessage = $"Dispute status updated to {request.NewStatus.Value}";
                    sendNotification = true;
                    notificationTitle = "Dispute Status Update";
                    notificationMessage = $"Your dispute status has been updated to: {request.NewStatus.Value}";
                    break;

                case DisputeManagementAction.UpdatePriority:
                    if (!request.NewPriority.HasValue)
                        return Result.Failure<DisputeDto>("NewPriority is required for update priority action");

                    dispute.Priority = request.NewPriority.Value;
                    actionMessage = $"Dispute priority updated to {request.NewPriority.Value}";
                    break;

                case DisputeManagementAction.Resolve:
                    if (string.IsNullOrEmpty(request.Resolution))
                        return Result.Failure<DisputeDto>("Resolution is required for resolve action");

                    dispute.Status = DisputeStatus.Resolved;
                    dispute.Resolution = request.Resolution;
                    dispute.ResolvedAt = DateTime.UtcNow;
                    actionMessage = "Dispute resolved successfully";
                    sendNotification = true;
                    notificationTitle = "Dispute Resolved";
                    notificationMessage = $"Your dispute has been resolved. Resolution: {request.Resolution}";
                    break;

                case DisputeManagementAction.Close:
                    dispute.Status = DisputeStatus.Closed;
                    if (!string.IsNullOrEmpty(request.Resolution) && string.IsNullOrEmpty(dispute.Resolution))
                    {
                        dispute.Resolution = request.Resolution;
                        dispute.ResolvedAt = DateTime.UtcNow;
                    }
                    actionMessage = "Dispute closed successfully";
                    sendNotification = true;
                    notificationTitle = "Dispute Closed";
                    notificationMessage = "Your dispute has been closed.";
                    break;

                case DisputeManagementAction.Escalate:
                    dispute.Status = DisputeStatus.Escalated;
                    dispute.Priority = DisputePriority.Critical;
                    actionMessage = "Dispute escalated successfully";
                    sendNotification = true;
                    notificationTitle = "Dispute Escalated";
                    notificationMessage = "Your dispute has been escalated to senior support staff.";
                    break;

                default:
                    return Result.Failure<DisputeDto>("Invalid action");
            }

            dispute.UpdatedAt = DateTime.UtcNow;
            dispute.UpdatedBy = request.AdminUserId.ToString();

            _unitOfWork.Repository<Dispute>().Update(dispute);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to user who raised the dispute
            if (sendNotification)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            dispute.RaisedBy,
                            notificationTitle,
                            notificationTitle,
                            notificationMessage,
                            notificationMessage,
                            NotificationCategory.System,
                            dispute.Id,
                            nameof(Dispute),
                            $"/disputes/{dispute.Id}",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending dispute management notification");
                    }
                }, cancellationToken);
            }

            _logger.LogInformation(
                "Admin {AdminId} performed {Action} on dispute {DisputeId}",
                request.AdminUserId, request.Action, request.DisputeId);

            var dto = new DisputeDto
            {
                Id = dispute.Id,
                BookingId = dispute.BookingId,
                ServiceName = dispute.Booking.Service.NameEn,
                RaisedBy = dispute.RaisedBy,
                RaisedByName = $"{dispute.RaisedByUser.FirstName} {dispute.RaisedByUser.LastName}",
                Type = dispute.Type.ToString(),
                Title = dispute.Title,
                Description = dispute.Description,
                Status = dispute.Status.ToString(),
                Priority = dispute.Priority.ToString(),
                AssignedTo = dispute.AssignedTo,
                AssignedToName = dispute.AssignedToUser != null
                    ? $"{dispute.AssignedToUser.FirstName} {dispute.AssignedToUser.LastName}"
                    : null,
                Resolution = dispute.Resolution,
                ResolvedAt = dispute.ResolvedAt,
                EvidenceUrls = dispute.EvidenceUrls?.ToList() ?? new List<string>(),
                CreatedAt = dispute.CreatedAt
            };

            return Result.Success(dto, actionMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing dispute {DisputeId}", request.DisputeId);
            return Result.Failure<DisputeDto>(
                "An error occurred while managing the dispute",
                ex.Message);
        }
    }
}
