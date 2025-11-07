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
        // Implementation will be added with handler
        return Ok(new { message = $"Get booking {id}" });
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] string reason)
    {
        // Implementation will be added with handler
        return Ok(new { message = $"Cancel booking {id}" });
    }
}
