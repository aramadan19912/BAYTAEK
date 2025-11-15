using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Report;

public class ReviewReportCommand : IRequest<Result<bool>>
{
    public Guid ReportId { get; set; }
    public Guid AdminUserId { get; set; }
    public ReportStatus Status { get; set; }
    public ModerationAction? ActionTaken { get; set; }
    public string? AdminNotes { get; set; }

    // Optional: Actions to take
    public bool SuspendUser { get; set; }
    public int? SuspensionDays { get; set; }
    public bool SendWarning { get; set; }
    public string? WarningMessage { get; set; }
}
