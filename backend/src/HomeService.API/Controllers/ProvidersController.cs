using HomeService.Application.Features.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeService.API.Controllers;

[Authorize]
public class ProvidersController : BaseApiController
{
    /// <summary>
    /// Search providers with filters (public)
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchProviders([FromQuery] Application.Queries.Provider.SearchProvidersQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider details by ID (public profile)
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderById(Guid id)
    {
        var query = new Application.Queries.Provider.GetProviderProfileQuery
        {
            ProviderId = id
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    #region Availability Management

    /// <summary>
    /// Provider: Set weekly availability schedule
    /// </summary>
    [HttpPost("availability/weekly")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> SetWeeklyAvailability(
        [FromBody] SetWeeklyAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new SetWeeklyAvailabilityCommand(userId, request.Schedule);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Block specific dates (time off, holidays)
    /// </summary>
    [HttpPost("availability/block")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> BlockDates(
        [FromBody] BlockDatesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new BlockDatesCommand(
            userId,
            request.StartDate,
            request.EndDate,
            request.Reason,
            request.IsAllDay
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Delete a blocked date
    /// </summary>
    [HttpDelete("availability/block/{blockedDateId:guid}")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> DeleteBlockedDate(Guid blockedDateId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new DeleteBlockedDateCommand(blockedDateId, userId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider availability (public or own)
    /// </summary>
    [HttpGet("{providerId:guid}/availability")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderAvailability(
        Guid providerId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProviderAvailabilityQuery(providerId, startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Earnings & Reports

    /// <summary>
    /// Provider: Get earnings dashboard
    /// </summary>
    [HttpGet("earnings")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetEarnings(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetProviderEarningsQuery(userId, startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Get detailed earnings report
    /// </summary>
    [HttpGet("earnings/report")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetEarningsReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetEarningsReportQuery(userId, startDate, endDate, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Portfolio Management

    /// <summary>
    /// Provider: Update service portfolio
    /// </summary>
    [HttpPut("portfolio")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> UpdatePortfolio(
        [FromBody] UpdatePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new UpdateServicePortfolioCommand(
            userId,
            request.Description,
            request.PortfolioImages,
            request.CertificationDocuments
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Performance & Analytics

    /// <summary>
    /// Provider: Get performance metrics
    /// </summary>
    [HttpGet("performance")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetProviderPerformanceQuery(userId, startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Get comprehensive dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetProviderDashboardQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}

// Request DTOs
public record SetWeeklyAvailabilityRequest(List<WeeklyScheduleDto> Schedule);

public record BlockDatesRequest(
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    bool IsAllDay = true
);

public record UpdatePortfolioRequest(
    string Description,
    List<string> PortfolioImages,
    List<string> CertificationDocuments
);
