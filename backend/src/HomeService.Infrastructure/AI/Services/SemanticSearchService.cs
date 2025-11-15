using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace HomeService.Infrastructure.AI.Services;

public class SemanticSearchService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public SemanticSearchService(SemanticKernelService semanticKernelService)
    {
        _kernel = semanticKernelService.GetKernel();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<SearchIntent> ParseSearchIntentAsync(
        string searchQuery,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var prompt = language.ToLower() == "ar"
            ? $@"حلل نية البحث التالية واستخرج:
               1. نوع الخدمة المطلوبة
               2. الموقع (إن وجد)
               3. الوقت المفضل (إن وجد)
               4. نطاق السعر (إن وجد)

               استعلام البحث: {searchQuery}

               أرجع إجابتك بهذا التنسيق:
               ServiceType: [نوع الخدمة]
               Location: [الموقع]
               TimePreference: [الوقت]
               PriceRange: [النطاق السعري]"
            : $@"Analyze the following search intent and extract:
               1. Service type requested
               2. Location (if mentioned)
               3. Time preference (if mentioned)
               4. Price range (if mentioned)

               Search Query: {searchQuery}

               Return your response in this format:
               ServiceType: [service type]
               Location: [location]
               TimePreference: [time]
               PriceRange: [price range]";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are a search intent parser for a home services platform.");
        chatHistory.AddUserMessage(prompt);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        return ParseSearchIntent(response.Content ?? string.Empty);
    }

    private SearchIntent ParseSearchIntent(string response)
    {
        var intent = new SearchIntent();
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("ServiceType:", StringComparison.OrdinalIgnoreCase))
            {
                intent.ServiceType = line.Substring("ServiceType:".Length).Trim();
            }
            else if (line.StartsWith("Location:", StringComparison.OrdinalIgnoreCase))
            {
                intent.Location = line.Substring("Location:".Length).Trim();
            }
            else if (line.StartsWith("TimePreference:", StringComparison.OrdinalIgnoreCase))
            {
                intent.TimePreference = line.Substring("TimePreference:".Length).Trim();
            }
            else if (line.StartsWith("PriceRange:", StringComparison.OrdinalIgnoreCase))
            {
                intent.PriceRange = line.Substring("PriceRange:".Length).Trim();
            }
        }

        return intent;
    }
}

public class SearchIntent
{
    public string ServiceType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? TimePreference { get; set; }
    public string? PriceRange { get; set; }
}
