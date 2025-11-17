using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Messages;

public record GetConversationMessagesQuery(
    Guid ConversationId,
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<Result<PagedResult<MessageDto>>>;

public class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, Result<PagedResult<MessageDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetConversationMessagesQueryHandler> _logger;

    public GetConversationMessagesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetConversationMessagesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<MessageDto>>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify user is participant
            var conversationRepo = _unitOfWork.Repository<Conversation>();
            var conversation = await conversationRepo
                .GetQueryable()
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
                return Result.Failure<PagedResult<MessageDto>>("Conversation not found");

            var isParticipant = conversation.Participants.Any(p => p.UserId == request.UserId);
            if (!isParticipant)
                return Result.Failure<PagedResult<MessageDto>>("User is not a participant in this conversation");

            // Get messages
            var messageRepo = _unitOfWork.Repository<Message>();
            var query = messageRepo
                .GetQueryable()
                .Where(m => m.ConversationId == request.ConversationId)
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var messages = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                Type = m.Type,
                AttachmentUrl = m.AttachmentUrl,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<MessageDto>
            {
                Items = messageDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", request.ConversationId);
            return Result.Failure<PagedResult<MessageDto>>(
                "An error occurred while retrieving messages",
                ex.Message);
        }
    }
}
