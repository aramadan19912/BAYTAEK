using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.SupportTicket;

public class GetAdminTicketsQuery : IRequest<Result<AdminTicketsDto>>
{
    public TicketStatus? Status { get; set; }
    public TicketCategory? Category { get; set; }
    public TicketPriority? Priority { get; set; }
    public Guid? UserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminTicketsDto
{
    public List<AdminTicketSummaryDto> Tickets { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Summary statistics
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int UnassignedTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int UrgentTickets { get; set; }
}

public class AdminTicketSummaryDto
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    // User info
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Related
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Stats
    public int MessagesCount { get; set; }
    public int UnreadByAdminCount { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
}
