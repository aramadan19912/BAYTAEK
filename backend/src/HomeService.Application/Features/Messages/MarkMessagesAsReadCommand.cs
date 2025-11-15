using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Messages;

public record MarkMessagesAsReadCommand(Guid ConversationId, Guid UserId) : IRequest<Result<bool>>;

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkMessagesAsReadCommandHandler> _logger;

    public MarkMessagesAsReadCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<MarkMessagesAsReadCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
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
                return Result.Failure<bool>("Conversation not found");

            var isParticipant = conversation.Participants.Any(p => p.UserId == request.UserId);
            if (!isParticipant)
                return Result.Failure<bool>("User is not a participant in this conversation");

            // Mark all unread messages as read
            var messageRepo = _unitOfWork.Repository<Message>();
            var unreadMessages = await messageRepo
                .GetQueryable()
                .Where(m => m.ConversationId == request.ConversationId
                    && m.SenderId != request.UserId
                    && !m.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                messageRepo.Update(message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked {Count} messages as read in conversation {ConversationId}",
                unreadMessages.Count, request.ConversationId);

            return Result.Success(true, $"Marked {unreadMessages.Count} messages as read");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read in conversation {ConversationId}",
                request.ConversationId);
            return Result.Failure<bool>("An error occurred while marking messages as read", ex.Message);
        }
    }
}
