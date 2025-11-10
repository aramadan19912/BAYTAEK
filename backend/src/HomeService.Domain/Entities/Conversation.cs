using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Conversation : AuditableEntity
{
    public Guid? BookingId { get; set; }
    public ConversationType Type { get; set; } = ConversationType.CustomerProvider;
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public bool IsArchived { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
