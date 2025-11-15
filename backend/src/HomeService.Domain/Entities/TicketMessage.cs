using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class TicketMessage : BaseEntity
{
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsStaffReply { get; set; }

    // Navigation properties
    public virtual SupportTicket Ticket { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
