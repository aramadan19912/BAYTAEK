using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Report;

public class GetReportsQuery : IRequest<Result<ReportsListDto>>
{
    public ReportStatus? Status { get; set; }
    public ReportType? Type { get; set; }
    public ReportReason? Reason { get; set; }
    public Guid? ReporterId { get; set; }
    public Guid? ReportedUserId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReportsListDto
{
    public List<ReportSummaryDto> Reports { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Summary statistics
    public int PendingReports { get; set; }
    public int UnderReviewReports { get; set; }
    public int ResolvedReports { get; set; }
    public int DismissedReports { get; set; }
}

public class ReportSummaryDto
{
    public Guid ReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportReason Reason { get; set; }
    public ReportStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;

    // Reporter info
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;

    // Reported entity info
    public Guid? ReportedUserId { get; set; }
    public string? ReportedUserName { get; set; }
    public Guid? ReportedBookingId { get; set; }
    public string? ReportedBookingNumber { get; set; }
    public Guid? ReportedReviewId { get; set; }
    public Guid? ReportedServiceId { get; set; }

    // Evidence
    public List<string> Evidence { get; set; } = new();

    // Review info
    public Guid? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public ModerationAction? ActionTaken { get; set; }
    public string? AdminNotes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
