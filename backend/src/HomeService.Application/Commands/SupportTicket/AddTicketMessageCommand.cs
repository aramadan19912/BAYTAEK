using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.SupportTicket;

public class AddTicketMessageCommand : IRequest<Result<TicketMessageAddedDto>>
{
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Attachments { get; set; }
    public bool IsFromUser { get; set; } = true;
}

public class TicketMessageAddedDto
{
    public Guid MessageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
