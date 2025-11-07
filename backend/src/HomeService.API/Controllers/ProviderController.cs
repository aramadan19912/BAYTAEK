using HomeService.Application.Queries.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize(Roles = "Provider")]
public class ProviderController : BaseApiController
{
    /// <summary>
    /// Get provider dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetProviderDashboardQuery
        {
            ProviderId = providerId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider earnings
    /// </summary>
    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetProviderEarningsQuery
        {
            ProviderId = providerId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider active bookings
    /// </summary>
    [HttpGet("bookings/active")]
    public async Task<IActionResult> GetActiveBookings()
    {
        // To be implemented
        return Ok(new { message = "Provider active bookings endpoint" });
    }

    /// <summary>
    /// Accept job request
    /// </summary>
    [HttpPost("bookings/{bookingId}/accept")]
    public async Task<IActionResult> AcceptJob(Guid bookingId)
    {
        // To be implemented
        return Ok(new { message = "Accept job endpoint" });
    }

    /// <summary>
    /// Decline job request
    /// </summary>
    [HttpPost("bookings/{bookingId}/decline")]
    public async Task<IActionResult> DeclineJob(Guid bookingId, [FromBody] DeclineJobRequest request)
    {
        // To be implemented
        return Ok(new { message = "Decline job endpoint" });
    }

    /// <summary>
    /// Update booking status (on the way, arrived, in progress, completed)
    /// </summary>
    [HttpPut("bookings/{bookingId}/status")]
    public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request)
    {
        // To be implemented
        return Ok(new { message = "Update booking status endpoint" });
    }

    /// <summary>
    /// Get provider profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // To be implemented
        return Ok(new { message = "Provider profile endpoint" });
    }

    /// <summary>
    /// Update provider availability
    /// </summary>
    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityRequest request)
    {
        // To be implemented
        return Ok(new { message = "Update availability endpoint" });
    }

    /// <summary>
    /// Toggle online/offline status
    /// </summary>
    [HttpPut("status/toggle")]
    public async Task<IActionResult> ToggleOnlineStatus([FromBody] ToggleStatusRequest request)
    {
        // To be implemented
        return Ok(new { message = "Toggle online status endpoint" });
    }

    /// <summary>
    /// Request payout
    /// </summary>
    [HttpPost("payouts/request")]
    public async Task<IActionResult> RequestPayout([FromBody] RequestPayoutRequest request)
    {
        // To be implemented
        return Ok(new { message = "Request payout endpoint" });
    }

    /// <summary>
    /// Get payout history
    /// </summary>
    [HttpGet("payouts/history")]
    public async Task<IActionResult> GetPayoutHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        // To be implemented
        return Ok(new { message = "Payout history endpoint" });
    }
}

// Request DTOs
public record DeclineJobRequest(string Reason, string? Notes);
public record UpdateBookingStatusRequest(string Status, string? Notes, List<string>? PhotoUrls);
public record UpdateAvailabilityRequest(bool IsAvailable, DateTime? AvailableFrom, DateTime? AvailableUntil);
public record ToggleStatusRequest(bool IsOnline);
public record RequestPayoutRequest(decimal Amount, bool IsInstant);
