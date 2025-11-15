using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace HomeService.Infrastructure.AI.Services;

public class RecommendationService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public RecommendationService(SemanticKernelService semanticKernelService)
    {
        _kernel = semanticKernelService.GetKernel();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<List<string>> GetServiceRecommendationsAsync(
        string userPreferences,
        List<string> bookingHistory,
        string location,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Based on the following user information, recommend 5 relevant home services:

        User Preferences: {userPreferences}
        Booking History: {string.Join(", ", bookingHistory)}
        Location: {location}

        Return ONLY a comma-separated list of service names without explanations.";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a home service recommendation engine. Provide relevant service recommendations based on user data.");
        chatHistory.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        var recommendations = response.Content?
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList() ?? new List<string>();

        return recommendations;
    }

    public async Task<decimal> PredictOptimalPriceAsync(
        string serviceName,
        string region,
        DateTime requestedTime,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Analyze the following and suggest an optimal price multiplier (between 0.8 and 2.0):

        Service: {serviceName}
        Region: {region}
        Requested Time: {requestedTime:yyyy-MM-dd HH:mm}
        Day of Week: {requestedTime.DayOfWeek}

        Consider:
        - Peak hours (evenings, weekends = higher multiplier)
        - Off-peak times (mornings, weekdays = lower multiplier)
        - Regional demand patterns

        Return ONLY a decimal number between 0.8 and 2.0";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a pricing optimization AI. Provide data-driven price recommendations.");
        chatHistory.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        if (decimal.TryParse(response.Content?.Trim(), out var multiplier))
        {
            return Math.Clamp(multiplier, 0.8m, 2.0m);
        }

        return 1.0m; // Default multiplier
    }
}
