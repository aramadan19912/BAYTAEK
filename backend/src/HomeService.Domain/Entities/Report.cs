using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class Report : AuditableEntity
{
    public Guid ReporterId { get; set; }
    public ReportType Type { get; set; }
    public Guid? ReportedUserId { get; set; }
    public Guid? ReportedServiceId { get; set; }
    public Guid? ReportedReviewId { get; set; }
    public Guid? BookingId { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public ModerationAction? Action { get; set; }

    // Navigation properties
    public virtual User Reporter { get; set; } = null!;
    public virtual User? ReportedUser { get; set; }
    public virtual Service? ReportedService { get; set; }
    public virtual Review? ReportedReview { get; set; }
    public virtual Booking? Booking { get; set; }
    public virtual User? ReviewedBy { get; set; }
}
