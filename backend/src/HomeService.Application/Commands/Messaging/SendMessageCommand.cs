using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Messaging;

public class SendMessageCommand : IRequest<Result<MessageSentDto>>
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid? BookingId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Attachments { get; set; }
}

public class MessageSentDto
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
