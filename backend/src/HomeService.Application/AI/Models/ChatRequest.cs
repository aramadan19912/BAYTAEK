namespace HomeService.Application.AI.Models;

/// <summary>
/// Represents a chat request from the user
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// The unique session identifier
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The user's message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The user's preferred language (en or ar)
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// The user ID (if authenticated)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Additional context for the chat
    /// </summary>
    public ChatContext? Context { get; set; }
}

/// <summary>
/// Additional context for the chat conversation
/// </summary>
public class ChatContext
{
    /// <summary>
    /// The current page or section the user is on
    /// </summary>
    public string? CurrentPage { get; set; }

    /// <summary>
    /// The user's current location
    /// </summary>
    public string? UserLocation { get; set; }

    /// <summary>
    /// The user's role (Customer, Provider, Admin)
    /// </summary>
    public string? UserRole { get; set; }

    /// <summary>
    /// Any relevant entity IDs (service, booking, etc.)
    /// </summary>
    public Dictionary<string, string>? EntityIds { get; set; }
}
