using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class ProviderBlockedDate : AuditableEntity
{
    public Guid ProviderId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }

    // Navigation properties
    public virtual ServiceProvider Provider { get; set; } = null!;
}
