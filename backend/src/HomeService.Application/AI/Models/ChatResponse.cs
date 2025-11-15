namespace HomeService.Application.AI.Models;

/// <summary>
/// Represents an AI-generated chat response
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// The session identifier
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The AI-generated message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Suggested actions or quick replies
    /// </summary>
    public List<SuggestedAction>? SuggestedActions { get; set; }

    /// <summary>
    /// Related services suggested by AI
    /// </summary>
    public List<ServiceSuggestion>? ServiceSuggestions { get; set; }

    /// <summary>
    /// Confidence score of the response (0-1)
    /// </summary>
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Represents a suggested action for the user
/// </summary>
public class SuggestedAction
{
    /// <summary>
    /// The display text for the action
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The action type (e.g., "navigate", "book_service", "contact_support")
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Additional data for the action
    /// </summary>
    public Dictionary<string, string>? Payload { get; set; }
}

/// <summary>
/// Represents a service suggestion from AI
/// </summary>
public class ServiceSuggestion
{
    /// <summary>
    /// The service ID
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// The service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// The base price
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// The currency
    /// </summary>
    public string Currency { get; set; } = "SAR";

    /// <summary>
    /// The average rating
    /// </summary>
    public double? AverageRating { get; set; }

    /// <summary>
    /// Why this service was suggested
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;
}

/// <summary>
/// Represents a chat message in the history
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// The message ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The session ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The message sender (user or assistant)
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// The message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Metadata about the message
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
