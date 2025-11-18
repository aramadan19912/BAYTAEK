using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Dispute : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Guid RaisedBy { get; set; } // Customer or Provider UserId
    public DisputeType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }
    public DisputePriority Priority { get; set; }
    public Guid? AssignedTo { get; set; } // Admin UserId
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string[]? EvidenceUrls { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual User RaisedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
}

