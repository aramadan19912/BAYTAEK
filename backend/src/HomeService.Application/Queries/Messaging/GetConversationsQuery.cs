using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Queries.Messaging;

public class GetConversationsQuery : IRequest<Result<ConversationsListDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ConversationsListDto
{
    public List<ConversationDto> Conversations { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
}

public class ConversationDto
{
    public Guid ConversationId { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserImage { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadMessagesCount { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
}
