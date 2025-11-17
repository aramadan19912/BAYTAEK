using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.SupportTicket;

public class UpdateTicketStatusCommand : IRequest<Result<bool>>
{
    public Guid TicketId { get; set; }
    public Guid AdminUserId { get; set; }
    public TicketStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignToUserId { get; set; }
}
