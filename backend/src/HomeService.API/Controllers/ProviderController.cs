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
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetProviderActiveBookingsQuery
        {
            ProviderId = providerId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Accept job request
    /// </summary>
    [HttpPost("bookings/{bookingId}/accept")]
    public async Task<IActionResult> AcceptJob(Guid bookingId, [FromBody] AcceptJobRequest? request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Booking.AcceptBookingCommand
        {
            BookingId = bookingId,
            ProviderId = providerId,
            EstimatedArrivalTime = request?.EstimatedArrivalTime,
            Notes = request?.Notes
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Decline job request
    /// </summary>
    [HttpPost("bookings/{bookingId}/decline")]
    public async Task<IActionResult> DeclineJob(Guid bookingId, [FromBody] DeclineJobRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Booking.DeclineBookingCommand
        {
            BookingId = bookingId,
            ProviderId = providerId,
            Reason = request.Reason,
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update booking status (on the way, arrived, in progress, completed)
    /// </summary>
    [HttpPut("bookings/{bookingId}/status")]
    public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Booking.UpdateBookingStatusCommand
        {
            BookingId = bookingId,
            ProviderId = providerId,
            Status = request.Status,
            Notes = request.Notes,
            PhotoUrls = request.PhotoUrls
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetProviderProfileQuery
        {
            ProviderId = providerId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update provider availability
    /// </summary>
    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Provider.UpdateProviderAvailabilityCommand
        {
            ProviderId = providerId,
            IsAvailable = request.IsAvailable,
            AvailableFrom = request.AvailableFrom,
            AvailableUntil = request.AvailableUntil
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Toggle online/offline status
    /// </summary>
    [HttpPut("status/toggle")]
    public async Task<IActionResult> ToggleOnlineStatus([FromBody] ToggleStatusRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Provider.ToggleOnlineStatusCommand
        {
            ProviderId = providerId,
            IsOnline = request.IsOnline
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Request payout
    /// </summary>
    [HttpPost("payouts/request")]
    public async Task<IActionResult> RequestPayout([FromBody] RequestPayoutRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Provider.RequestPayoutCommand
        {
            ProviderId = providerId,
            Amount = request.Amount,
            IsInstant = request.IsInstant
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get payout history
    /// </summary>
    [HttpGet("payouts/history")]
    public async Task<IActionResult> GetPayoutHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetPayoutHistoryQuery
        {
            ProviderId = providerId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all services offered by the provider
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetMyServices([FromQuery] bool? isActive)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new Application.Queries.ProviderService.GetProviderServicesQuery
        {
            ProviderId = providerId,
            IsActive = isActive
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new service (requires admin approval)
    /// </summary>
    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.ProviderService.CreateProviderServiceCommand
        {
            ProviderId = providerId,
            CategoryId = request.CategoryId,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            BasePrice = request.BasePrice,
            Currency = request.Currency,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            AvailableRegions = request.AvailableRegions,
            RequiredMaterials = request.RequiredMaterials,
            WarrantyInfo = request.WarrantyInfo,
            ImageUrls = request.ImageUrls,
            VideoUrl = request.VideoUrl
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetMyServices), new { }, result);
    }

    /// <summary>
    /// Update an existing service
    /// </summary>
    [HttpPut("services/{serviceId}")]
    public async Task<IActionResult> UpdateService(Guid serviceId, [FromBody] UpdateServiceRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.ProviderService.UpdateProviderServiceCommand
        {
            ServiceId = serviceId,
            ProviderId = providerId,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            BasePrice = request.BasePrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            AvailableRegions = request.AvailableRegions,
            RequiredMaterials = request.RequiredMaterials,
            WarrantyInfo = request.WarrantyInfo,
            ImageUrls = request.ImageUrls,
            VideoUrl = request.VideoUrl
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a service (only if no active bookings)
    /// </summary>
    [HttpDelete("services/{serviceId}")]
    public async Task<IActionResult> DeleteService(Guid serviceId)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.ProviderService.DeleteProviderServiceCommand
        {
            ServiceId = serviceId,
            ProviderId = providerId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record AcceptJobRequest(DateTime? EstimatedArrivalTime, string? Notes);
public record DeclineJobRequest(string Reason, string? Notes);
public record UpdateBookingStatusRequest(string Status, string? Notes, List<string>? PhotoUrls);
public record UpdateAvailabilityRequest(bool IsAvailable, DateTime? AvailableFrom, DateTime? AvailableUntil);
public record ToggleStatusRequest(bool IsOnline);
public record RequestPayoutRequest(decimal Amount, bool IsInstant);
public record CreateServiceRequest(
    Guid CategoryId,
    string NameEn,
    string NameAr,
    string DescriptionEn,
    string DescriptionAr,
    decimal BasePrice,
    string Currency,
    int EstimatedDurationMinutes,
    List<HomeService.Domain.Enums.Region> AvailableRegions,
    string? RequiredMaterials,
    string? WarrantyInfo,
    List<string>? ImageUrls,
    string? VideoUrl);
public record UpdateServiceRequest(
    string? NameEn,
    string? NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal? BasePrice,
    int? EstimatedDurationMinutes,
    List<HomeService.Domain.Enums.Region>? AvailableRegions,
    string? RequiredMaterials,
    string? WarrantyInfo,
    List<string>? ImageUrls,
    string? VideoUrl);
