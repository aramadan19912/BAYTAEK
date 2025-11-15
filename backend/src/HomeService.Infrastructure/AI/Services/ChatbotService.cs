using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace HomeService.Infrastructure.AI.Services;

public class ChatbotService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public ChatbotService(SemanticKernelService semanticKernelService)
    {
        _kernel = semanticKernelService.GetKernel();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> GetResponseAsync(string userMessage, string language = "en", CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();

        // System prompt based on language
        var systemPrompt = language.ToLower() == "ar"
            ? @"أنت مساعد ذكي لخدمة العملاء في تطبيق الخدمات المنزلية.
               مهمتك مساعدة العملاء في:
               - البحث عن الخدمات المتاحة
               - حجز الخدمات
               - الإجابة على الأسئلة الشائعة
               - حل المشكلات البسيطة
               كن مهذباً ومساعداً ومحترفاً في جميع الأوقات."
            : @"You are an intelligent customer service assistant for a home services application.
               Your role is to help customers with:
               - Finding available services
               - Booking services
               - Answering frequently asked questions
               - Resolving simple issues
               Be polite, helpful, and professional at all times.";

        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userMessage);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        return response.Content ?? string.Empty;
    }

    public async Task<string> GetContextualResponseAsync(
        string userMessage,
        List<string> conversationHistory,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();

        // Add system message
        var systemPrompt = language.ToLower() == "ar"
            ? "أنت مساعد ذكي للخدمات المنزلية. استخدم السياق السابق للمحادثة للرد بشكل أفضل."
            : "You are an intelligent home services assistant. Use the previous conversation context to respond better.";

        chatHistory.AddSystemMessage(systemPrompt);

        // Add conversation history
        foreach (var message in conversationHistory)
        {
            chatHistory.AddUserMessage(message);
        }

        // Add current message
        chatHistory.AddUserMessage(userMessage);

        var response = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        return response.Content ?? string.Empty;
    }
}
