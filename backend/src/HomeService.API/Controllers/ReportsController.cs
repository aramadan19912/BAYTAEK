using HomeService.Application.Commands.Report;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class ReportsController : BaseApiController
{
    /// <summary>
    /// Submit a report (User)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new CreateReportCommand
        {
            ReporterId = userId,
            Type = request.Type,
            Reason = request.Reason,
            Description = request.Description,
            ReportedUserId = request.ReportedUserId,
            ReportedBookingId = request.ReportedBookingId,
            ReportedReviewId = request.ReportedReviewId,
            ReportedServiceId = request.ReportedServiceId,
            Evidence = request.Evidence
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(CreateReport), new { }, result);
    }
}

// Request DTOs
public record CreateReportRequest(
    ReportType Type,
    ReportReason Reason,
    string Description,
    Guid? ReportedUserId,
    Guid? ReportedBookingId,
    Guid? ReportedReviewId,
    Guid? ReportedServiceId,
    List<string>? Evidence);
