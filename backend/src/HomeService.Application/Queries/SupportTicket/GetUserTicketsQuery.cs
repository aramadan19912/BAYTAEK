using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.SupportTicket;

public class GetUserTicketsQuery : IRequest<Result<UserTicketsDto>>
{
    public Guid UserId { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketCategory? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserTicketsDto
{
    public List<TicketSummaryDto> Tickets { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Summary statistics
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
}

public class TicketSummaryDto
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int MessagesCount { get; set; }
    public int UnreadMessagesCount { get; set; }
    public string? LastMessagePreview { get; set; }
    public string? AssignedToName { get; set; }
}
