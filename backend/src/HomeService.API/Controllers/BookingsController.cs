using HomeService.Application.Commands.Bookings;
using HomeService.Application.Features.Bookings;
using HomeService.Application.Queries.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeService.API.Controllers;

[Authorize]
public class BookingsController : BaseApiController
{
    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetBookingById), new { id = result.Data.Id }, result);
    }

    /// <summary>
    /// Get user's bookings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserBookings([FromQuery] GetUserBookingsQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get booking by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var userId = GetCurrentUserId();

        var query = new Application.Queries.Booking.GetBookingByIdQuery
        {
            BookingId = id,
            UserId = userId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get customer's booking history with advanced filtering and statistics
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetBookingHistory(
        [FromQuery] HomeService.Domain.Enums.BookingStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var customerId = GetCurrentUserId();

        var query = new Application.Queries.CustomerBooking.GetCustomerBookingsQuery
        {
            CustomerId = customerId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get detailed booking information with timeline
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetBookingDetails(Guid id)
    {
        var customerId = GetCurrentUserId();

        var query = new Application.Queries.CustomerBooking.GetBookingDetailQuery
        {
            BookingId = id,
            CustomerId = customerId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new CancelBookingCommand(
            id,
            userId,
            request.Reason,
            request.IsCustomerCancellation
        );

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Accept a booking
    /// </summary>
    [HttpPost("{id}/accept")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> AcceptBooking(Guid id, [FromBody] AcceptBookingRequest? request = null)
    {
        var providerId = GetCurrentUserId();
        if (providerId == Guid.Empty)
            return Unauthorized();

        var command = new AcceptBookingCommand(id, providerId, request?.EstimatedDurationMinutes);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Reject a booking
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> RejectBooking(Guid id, [FromBody] RejectBookingRequest request)
    {
        var providerId = GetCurrentUserId();
        if (providerId == Guid.Empty)
            return Unauthorized();

        var command = new RejectBookingCommand(id, providerId, request.Reason);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Start service
    /// </summary>
    [HttpPost("{id}/start")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> StartService(Guid id, [FromBody] StartServiceRequest? request = null)
    {
        var providerId = GetCurrentUserId();
        if (providerId == Guid.Empty)
            return Unauthorized();

        var command = new StartServiceCommand(
            id,
            providerId,
            request?.Latitude,
            request?.Longitude
        );

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider: Complete service
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> CompleteService(Guid id, [FromBody] CompleteServiceRequest request)
    {
        var providerId = GetCurrentUserId();
        if (providerId == Guid.Empty)
            return Unauthorized();

        var command = new CompleteServiceCommand(
            id,
            providerId,
            request.CompletionPhotoUrls,
            request.Notes
        );

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reschedule a booking
    /// </summary>
    [HttpPost("{id}/reschedule")]
    public async Task<IActionResult> RescheduleBooking(Guid id, [FromBody] RescheduleBookingRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new RescheduleBookingCommand(
            id,
            userId,
            request.NewScheduledAt,
            request.Reason,
            request.IsCustomerRequest
        );

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}

// Request DTOs
public record CancelBookingRequest(string Reason, bool IsCustomerCancellation = true);
public record AcceptBookingRequest(int? EstimatedDurationMinutes = null);
public record RejectBookingRequest(string Reason);
public record StartServiceRequest(double? Latitude = null, double? Longitude = null);
public record CompleteServiceRequest(List<string>? CompletionPhotoUrls = null, string? Notes = null);
public record RescheduleBookingRequest(
    DateTime NewScheduledAt,
    string? Reason = null,
    bool IsCustomerRequest = true
);
