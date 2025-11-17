using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Messages;

public record SendMessageCommand : IRequest<Result<MessageDto>>
{
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; } = MessageType.Text;
    public string? AttachmentUrl { get; init; }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(IUnitOfWork unitOfWork, ILogger<SendMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify conversation exists and user is participant
            var conversationRepo = _unitOfWork.Repository<Conversation>();
            var conversation = await conversationRepo
                .GetQueryable()
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
                return Result.Failure<MessageDto>("Conversation not found");

            var isParticipant = conversation.Participants.Any(p => p.UserId == request.SenderId);
            if (!isParticipant)
                return Result.Failure<MessageDto>("User is not a participant in this conversation");

            // Create message
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                Type = request.Type,
                AttachmentUrl = request.AttachmentUrl,
                IsRead = false
            };

            await _unitOfWork.Repository<Message>().AddAsync(message, cancellationToken);

            // Update conversation last message timestamp
            conversation.LastMessageAt = DateTime.UtcNow;
            conversationRepo.Update(conversation);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Message sent in conversation {ConversationId} by user {SenderId}",
                request.ConversationId, request.SenderId);

            var messageDto = new MessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content,
                Type = message.Type,
                AttachmentUrl = message.AttachmentUrl,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };

            return Result.Success(messageDto, "Message sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in conversation {ConversationId}", request.ConversationId);
            return Result.Failure<MessageDto>("An error occurred while sending the message", ex.Message);
        }
    }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
