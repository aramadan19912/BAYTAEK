using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace HomeService.Infrastructure.AI.Services;

public class SentimentAnalysisService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public SentimentAnalysisService(SemanticKernelService semanticKernelService)
    {
        _kernel = semanticKernelService.GetKernel();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<SentimentResult> AnalyzeSentimentAsync(string text, CancellationToken cancellationToken = default)
    {
        var prompt = $@"Analyze the sentiment of the following review text and provide:
        1. Sentiment score from -1.0 (very negative) to 1.0 (very positive)
        2. Overall sentiment classification: Positive, Neutral, or Negative
        3. Key themes or topics mentioned

        Review: {text}

        Return your response in this exact format:
        Score: [number]
        Classification: [Positive/Neutral/Negative]
        Themes: [comma-separated themes]";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a sentiment analysis expert. Analyze customer reviews accurately.");
        chatHistory.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        return ParseSentimentResponse(response.Content ?? string.Empty);
    }

    private SentimentResult ParseSentimentResponse(string response)
    {
        var result = new SentimentResult();

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("Score:", StringComparison.OrdinalIgnoreCase))
            {
                var scoreText = line.Substring("Score:".Length).Trim();
                if (decimal.TryParse(scoreText, out var score))
                {
                    result.Score = Math.Clamp(score, -1.0m, 1.0m);
                }
            }
            else if (line.StartsWith("Classification:", StringComparison.OrdinalIgnoreCase))
            {
                result.Classification = line.Substring("Classification:".Length).Trim();
            }
            else if (line.StartsWith("Themes:", StringComparison.OrdinalIgnoreCase))
            {
                var themesText = line.Substring("Themes:".Length).Trim();
                result.Themes = themesText.Split(',').Select(t => t.Trim()).ToList();
            }
        }

        return result;
    }
}

public class SentimentResult
{
    public decimal Score { get; set; }
    public string Classification { get; set; } = "Neutral";
    public List<string> Themes { get; set; } = new();
}
