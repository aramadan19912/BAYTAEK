using HomeService.Application.Commands.SupportTicket;
using HomeService.Application.Queries.SupportTicket;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class SupportTicketsController : BaseApiController
{
    /// <summary>
    /// Create a new support ticket
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new CreateTicketCommand
        {
            UserId = userId,
            Subject = request.Subject,
            Description = request.Description,
            Category = request.Category,
            Priority = request.Priority,
            BookingId = request.BookingId,
            Attachments = request.Attachments
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetTicketDetail), new { id = result.Data.TicketId }, result);
    }

    /// <summary>
    /// Get user's support tickets
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketCategory? category,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetUserTicketsQuery
        {
            UserId = userId,
            Status = status,
            Category = category,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get ticket details with message history
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketDetail(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new GetTicketDetailQuery
        {
            TicketId = id,
            UserId = userId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Add a message to a support ticket
    /// </summary>
    [HttpPost("{id}/messages")]
    public async Task<IActionResult> AddTicketMessage(Guid id, [FromBody] AddTicketMessageRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new AddTicketMessageCommand
        {
            TicketId = id,
            UserId = userId,
            Message = request.Message,
            Attachments = request.Attachments,
            IsFromUser = true
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record CreateTicketRequest(
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    Guid? BookingId,
    List<string>? Attachments);

public record AddTicketMessageRequest(
    string Message,
    List<string>? Attachments);
