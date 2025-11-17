using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Notifications;

public record GetUnreadCountQuery(Guid UserId) : IRequest<Result<UnreadCountDto>>;

public class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, Result<UnreadCountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;

    public GetUnreadCountQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUnreadCountQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UnreadCountDto>> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var unreadCount = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == request.UserId && !n.IsRead)
                .CountAsync(cancellationToken);

            return Result.Success(new UnreadCountDto { Count = unreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", request.UserId);
            return Result.Failure<UnreadCountDto>("An error occurred while getting unread count", ex.Message);
        }
    }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
