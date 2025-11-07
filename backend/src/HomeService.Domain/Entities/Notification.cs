using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Notification : AuditableEntity
{
    public Guid UserId { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
