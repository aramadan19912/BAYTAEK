using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Messages;

public record GetConversationsQuery(Guid UserId, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ConversationDto>>>;

public class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, Result<PagedResult<ConversationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetConversationsQueryHandler> _logger;

    public GetConversationsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetConversationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ConversationDto>>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var conversationRepo = _unitOfWork.Repository<Conversation>();

            var query = conversationRepo
                .GetQueryable()
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.Participants.Any(p => p.UserId == request.UserId))
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var conversations = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var conversationDtos = conversations.Select(c =>
            {
                var lastMessage = c.Messages.FirstOrDefault();
                var otherParticipant = c.Participants.FirstOrDefault(p => p.UserId != request.UserId);

                return new ConversationDto
                {
                    Id = c.Id,
                    Type = c.Type,
                    BookingId = c.BookingId,
                    LastMessageAt = c.LastMessageAt,
                    LastMessage = lastMessage != null ? new MessageDto
                    {
                        Id = lastMessage.Id,
                        ConversationId = lastMessage.ConversationId,
                        SenderId = lastMessage.SenderId,
                        Content = lastMessage.Content,
                        Type = lastMessage.Type,
                        AttachmentUrl = lastMessage.AttachmentUrl,
                        IsRead = lastMessage.IsRead,
                        CreatedAt = lastMessage.CreatedAt
                    } : null,
                    OtherParticipant = otherParticipant != null ? new ParticipantDto
                    {
                        UserId = otherParticipant.UserId,
                        DisplayName = $"{otherParticipant.User.FirstName} {otherParticipant.User.LastName}",
                        ProfileImageUrl = otherParticipant.User.ProfileImageUrl
                    } : null,
                    UnreadCount = c.Messages.Count(m => m.SenderId != request.UserId && !m.IsRead),
                    CreatedAt = c.CreatedAt
                };
            }).ToList();

            var pagedResult = new PagedResult<ConversationDto>
            {
                Items = conversationDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user {UserId}", request.UserId);
            return Result.Failure<PagedResult<ConversationDto>>(
                "An error occurred while retrieving conversations",
                ex.Message);
        }
    }
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public MessageDto? LastMessage { get; set; }
    public ParticipantDto? OtherParticipant { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
}
