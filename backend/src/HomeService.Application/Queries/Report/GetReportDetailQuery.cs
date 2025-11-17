using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Report;

public class GetReportDetailQuery : IRequest<Result<ReportDetailDto>>
{
    public Guid ReportId { get; set; }
}

public class ReportDetailDto
{
    public Guid ReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportReason Reason { get; set; }
    public ReportStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;

    // Reporter details
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string? ReporterPhone { get; set; }
    public UserRole ReporterRole { get; set; }

    // Reported entity details
    public Guid? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public string? ReportedUserEmail { get; set; }
    public UserRole? ReportedUserRole { get; set; }
    public int? ReportedUserViolationCount { get; set; }

    public Guid? ReportedBookingId { get; set; }
    public string? ReportedBookingNumber { get; set; }
    public BookingStatus? ReportedBookingStatus { get; set; }

    public Guid? ReportedReviewId { get; set; }
    public int? ReportedReviewRating { get; set; }
    public string? ReportedReviewComment { get; set; }

    public Guid? ReportedServiceId { get; set; }
    public string? ReportedServiceName { get; set; }

    // Evidence
    public List<string> Evidence { get; set; } = new();

    // Review information
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public ModerationAction? ActionTaken { get; set; }
    public string? AdminNotes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Related reports (if any)
    public int RelatedReportsCount { get; set; }
}
