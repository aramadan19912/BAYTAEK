using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Report;

public class CreateReportCommand : IRequest<Result<ReportCreatedDto>>
{
    public Guid ReporterId { get; set; }
    public ReportType Type { get; set; }
    public ReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;

    // What is being reported
    public Guid? ReportedUserId { get; set; }
    public Guid? ReportedBookingId { get; set; }
    public Guid? ReportedReviewId { get; set; }
    public Guid? ReportedServiceId { get; set; }

    public List<string>? Evidence { get; set; } // Screenshots, images
}

public class ReportCreatedDto
{
    public Guid ReportId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
