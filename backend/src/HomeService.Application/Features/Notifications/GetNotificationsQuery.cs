using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public record GetNotificationsQuery(
    Guid UserId,
    bool? IsRead = null,
    NotificationType? Type = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<NotificationDto>>>;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetNotificationsQueryHandler> _logger;

    public GetNotificationsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetNotificationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == request.UserId);

            // Apply filters
            if (request.IsRead.HasValue)
                query = query.Where(n => n.IsRead == request.IsRead.Value);

            if (request.Type.HasValue)
                query = query.Where(n => n.Type == request.Type.Value);

            // Order by creation date (most recent first)
            query = query.OrderByDescending(n => n.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var notifications = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                TitleEn = n.TitleEn,
                TitleAr = n.TitleAr,
                MessageEn = n.MessageEn,
                MessageAr = n.MessageAr,
                Type = n.Type,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId,
                ActionUrl = n.ActionUrl,
                CreatedAt = n.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<NotificationDto>
            {
                Items = notificationDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", request.UserId);
            return Result.Failure<PagedResult<NotificationDto>>(
                "An error occurred while retrieving notifications",
                ex.Message);
        }
    }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
