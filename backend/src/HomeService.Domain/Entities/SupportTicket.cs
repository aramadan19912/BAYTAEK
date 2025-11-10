using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class SupportTicket : AuditableEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? BookingId { get; set; }
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedById { get; set; }
    public string? ResolutionNotes { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
    public virtual User? AssignedTo { get; set; }
    public virtual User? ResolvedBy { get; set; }
    public virtual ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}
