using HomeService.Application.Features.Messages;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HomeService.API.Controllers;

[Authorize]
public class MessagesController : BaseApiController
{
    private readonly IHubContext<Hubs.ChatHub> _chatHub;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IHubContext<Hubs.ChatHub> chatHub,
        ILogger<MessagesController> logger)
    {
        _chatHub = chatHub;
        _logger = logger;
    }

    /// <summary>
    /// Get all conversations for the current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetConversationsQuery(userId, pageNumber, pageSize);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific conversation by ID
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetConversationsQuery(userId, 1, 100);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        var conversation = result.Data?.Items.FirstOrDefault(c => c.Id == conversationId);
        if (conversation == null)
            return NotFound(new { message = "Conversation not found" });

        return Ok(new { isSuccess = true, data = conversation });
    }

    /// <summary>
    /// Create a new conversation
    /// </summary>
    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new CreateConversationCommand
        {
            InitiatorId = userId,
            ParticipantId = request.ParticipantId,
            Type = request.Type,
            BookingId = request.BookingId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get messages in a conversation
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetConversationMessagesQuery(conversationId, userId, pageNumber, pageSize);
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send a message in a conversation
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new SendMessageCommand
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = request.Content,
            Type = request.Type,
            AttachmentUrl = request.AttachmentUrl
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        // Send real-time notification via SignalR
        try
        {
            await _chatHub.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    messageId = result.Data!.Id,
                    conversationId = conversationId,
                    senderId = userId,
                    content = result.Data.Content,
                    type = result.Data.Type.ToString(),
                    attachmentUrl = result.Data.AttachmentUrl,
                    timestamp = result.Data.CreatedAt
                });

            _logger.LogInformation("Real-time message notification sent for conversation {ConversationId}",
                conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time notification for message");
            // Don't fail the request if SignalR notification fails
        }

        return Ok(result);
    }

    /// <summary>
    /// Mark messages as read in a conversation
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new MarkMessagesAsReadCommand(conversationId, userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        // Send real-time notification via SignalR
        try
        {
            await _chatHub.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("MessagesRead", new
                {
                    conversationId = conversationId,
                    userId = userId,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("Real-time read notification sent for conversation {ConversationId}",
                conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time read notification");
        }

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
public record CreateConversationRequest(
    Guid ParticipantId,
    ConversationType Type = ConversationType.Direct,
    Guid? BookingId = null
);

public record SendMessageRequest(
    string Content,
    MessageType Type = MessageType.Text,
    string? AttachmentUrl = null
);
