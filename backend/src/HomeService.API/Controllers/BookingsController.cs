using HomeService.Application.Commands.Bookings;
using HomeService.Application.Queries.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

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
        var customerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

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
        var customerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

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
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Booking.CancelBookingCommand
        {
            BookingId = id,
            UserId = userId,
            Reason = request.Reason,
            IsCustomerCancellation = true
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record CancelBookingRequest(string Reason);
