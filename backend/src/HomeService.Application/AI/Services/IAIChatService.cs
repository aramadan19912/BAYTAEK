using HomeService.Application.AI.Models;

namespace HomeService.Application.AI.Services;

/// <summary>
/// Service for AI-powered chat functionality
/// </summary>
public interface IAIChatService
{
    /// <summary>
    /// Sends a chat message and receives an AI response
    /// </summary>
    /// <param name="request">The chat request containing the message and context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The AI-generated response</returns>
    Task<ChatResponse> SendMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chat history for a specific session
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of chat messages in the session</returns>
    Task<List<ChatMessage>> GetChatHistoryAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears chat history for a session
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearChatHistoryAsync(string sessionId, CancellationToken cancellationToken = default);
}
