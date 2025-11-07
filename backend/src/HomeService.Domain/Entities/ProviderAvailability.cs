using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class ProviderAvailability : AuditableEntity
{
    public Guid ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }

    // Navigation properties
    public virtual ServiceProvider Provider { get; set; } = null!;
}
