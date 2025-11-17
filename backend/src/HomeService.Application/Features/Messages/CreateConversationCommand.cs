using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Messages;

public record CreateConversationCommand : IRequest<Result<ConversationDto>>
{
    public Guid InitiatorId { get; init; }
    public Guid ParticipantId { get; init; }
    public ConversationType Type { get; init; } = ConversationType.Direct;
    public Guid? BookingId { get; init; }
}

public class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, Result<ConversationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateConversationCommandHandler> _logger;

    public CreateConversationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateConversationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ConversationDto>> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var conversationRepo = _unitOfWork.Repository<Conversation>();

            // Check if conversation already exists between these users
            var existingConversation = await conversationRepo
                .GetQueryable()
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Where(c => c.Type == request.Type)
                .Where(c => c.Participants.Any(p => p.UserId == request.InitiatorId))
                .Where(c => c.Participants.Any(p => p.UserId == request.ParticipantId))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConversation != null)
            {
                // Return existing conversation
                var otherParticipant = existingConversation.Participants
                    .FirstOrDefault(p => p.UserId != request.InitiatorId);

                return Result.Success(new ConversationDto
                {
                    Id = existingConversation.Id,
                    Type = existingConversation.Type,
                    BookingId = existingConversation.BookingId,
                    LastMessageAt = existingConversation.LastMessageAt,
                    OtherParticipant = otherParticipant != null ? new ParticipantDto
                    {
                        UserId = otherParticipant.UserId,
                        DisplayName = $"{otherParticipant.User.FirstName} {otherParticipant.User.LastName}",
                        ProfileImageUrl = otherParticipant.User.ProfileImageUrl
                    } : null,
                    UnreadCount = 0,
                    CreatedAt = existingConversation.CreatedAt
                }, "Conversation already exists");
            }

            // Create new conversation
            var conversation = new Conversation
            {
                Type = request.Type,
                BookingId = request.BookingId,
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = request.InitiatorId },
                    new ConversationParticipant { UserId = request.ParticipantId }
                }
            };

            await conversationRepo.AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with user details
            var createdConversation = await conversationRepo
                .GetQueryable()
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id, cancellationToken);

            var participant = createdConversation!.Participants
                .FirstOrDefault(p => p.UserId != request.InitiatorId);

            _logger.LogInformation("Conversation created between users {InitiatorId} and {ParticipantId}",
                request.InitiatorId, request.ParticipantId);

            return Result.Success(new ConversationDto
            {
                Id = createdConversation.Id,
                Type = createdConversation.Type,
                BookingId = createdConversation.BookingId,
                LastMessageAt = createdConversation.LastMessageAt,
                OtherParticipant = participant != null ? new ParticipantDto
                {
                    UserId = participant.UserId,
                    DisplayName = $"{participant.User.FirstName} {participant.User.LastName}",
                    ProfileImageUrl = participant.User.ProfileImageUrl
                } : null,
                UnreadCount = 0,
                CreatedAt = createdConversation.CreatedAt
            }, "Conversation created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return Result.Failure<ConversationDto>("An error occurred while creating the conversation", ex.Message);
        }
    }
}
