using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.SupportTicket;

public class GetTicketDetailQuery : IRequest<Result<TicketDetailDto>>
{
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; } // For authorization
}

public class TicketDetailDto
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    // Related info
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }

    // User info
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    // Assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Attachments
    public List<string> Attachments { get; set; } = new();

    // Messages
    public List<TicketMessageDto> Messages { get; set; } = new();

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Resolution
    public string? ResolutionNotes { get; set; }
}

public class TicketMessageDto
{
    public Guid MessageId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public List<string> Attachments { get; set; } = new();
    public bool IsReadByUser { get; set; }
    public bool IsReadByAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
}
