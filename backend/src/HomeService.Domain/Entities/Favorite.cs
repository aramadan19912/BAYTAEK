using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class Favorite : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
