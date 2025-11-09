# AI Features with Microsoft Semantic Kernel

This module provides AI-powered features for the Home Service platform using Microsoft Semantic Kernel.

## Features

### 1. Intelligent Chatbot
- **24/7 multilingual support** (Arabic & English)
- Context-aware responses based on user location, role, and current page
- Service suggestions and quick actions
- Chat history tracking
- Session management

### 2. Service Recommendations
- **Personalized recommendations** based on user history and preferences
- **Similar services** suggestions
- **Trending services** by location
- Natural language query-based recommendations
- Confidence scores and recommendation reasons

### 3. Semantic Search
- Natural language service search
- Multi-language support (Arabic & English)
- Intent understanding and query interpretation
- Search suggestions and autocomplete
- Help/FAQ semantic search
- Relevance scoring and match highlighting

## Architecture

### Services

#### IAIChatService
Handles conversational AI interactions with users.

**Methods:**
- `SendMessageAsync()` - Processes user messages and generates responses
- `GetChatHistoryAsync()` - Retrieves conversation history
- `ClearChatHistoryAsync()` - Clears session history

#### IServiceRecommendationService
Provides intelligent service recommendations.

**Methods:**
- `GetPersonalizedRecommendationsAsync()` - User-specific recommendations
- `GetSimilarServicesAsync()` - Find similar services
- `GetTrendingServicesAsync()` - Popular services by location
- `GetRecommendationsFromQueryAsync()` - Query-based recommendations

#### ISemanticSearchService
Enables natural language search capabilities.

**Methods:**
- `SearchServicesAsync()` - Semantic service search
- `GetSearchSuggestionsAsync()` - Autocomplete suggestions
- `SearchHelpContentAsync()` - FAQ/help search

### Models

#### Chat Models
- `ChatRequest` - User chat input with context
- `ChatResponse` - AI-generated response with suggestions
- `ChatMessage` - Individual message in conversation
- `ChatContext` - Additional context (page, location, role)
- `SuggestedAction` - Quick action buttons
- `ServiceSuggestion` - Recommended services in chat

#### Recommendation Models
- `ServiceRecommendation` - Service with recommendation score
- Includes: service details, ratings, matched features, availability

#### Search Models
- `SemanticSearchResult` - Search results with intent interpretation
- `ServiceSearchResult` - Individual result with relevance score
- `SearchFilters` - Filter criteria (price, rating, location, category)
- `HelpArticle` - FAQ/help content with relevance score

## Setup and Configuration

### 1. Install Required Packages

```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI
dotnet add package Azure.AI.OpenAI
```

### 2. Configuration

Add to `appsettings.json`:

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "Model": "gpt-4",
      "EmbeddingModel": "text-embedding-ada-002",
      "MaxTokens": 1000,
      "Temperature": 0.7
    },
    "Features": {
      "ChatbotEnabled": true,
      "RecommendationsEnabled": true,
      "SemanticSearchEnabled": true
    },
    "Prompts": {
      "SystemPrompt": "You are a helpful assistant for a home services platform. Assist users with booking services, finding providers, and answering questions. Respond in the user's language (Arabic or English).",
      "ChatContextTemplate": "User Role: {role}, Location: {location}, Page: {page}"
    }
  }
}
```

### 3. Dependency Injection

Register services in `Program.cs`:

```csharp
// Add Semantic Kernel
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["AI:OpenAI:ApiKey"];
    var model = config["AI:OpenAI:Model"];

    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddOpenAIChatCompletion(model, apiKey);
    kernelBuilder.AddOpenAITextEmbeddingGeneration(
        config["AI:OpenAI:EmbeddingModel"],
        apiKey);

    return kernelBuilder.Build();
});

// Register AI services
builder.Services.AddScoped<IAIChatService, AIChatService>();
builder.Services.AddScoped<IServiceRecommendationService, ServiceRecommendationService>();
builder.Services.AddScoped<ISemanticSearchService, SemanticSearchService>();
```

## Usage Examples

### Chatbot

```csharp
public class ChatController : ControllerBase
{
    private readonly IAIChatService _chatService;

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        var response = await _chatService.SendMessageAsync(request);
        return Ok(response);
    }

    [HttpGet("chat/{sessionId}/history")]
    public async Task<ActionResult<List<ChatMessage>>> GetHistory(string sessionId)
    {
        var history = await _chatService.GetChatHistoryAsync(sessionId);
        return Ok(history);
    }
}
```

### Service Recommendations

```csharp
public class RecommendationsController : ControllerBase
{
    private readonly IServiceRecommendationService _recommendationService;

    [HttpGet("users/{userId}/recommendations")]
    public async Task<ActionResult<List<ServiceRecommendation>>> GetRecommendations(
        Guid userId,
        [FromQuery] int count = 10)
    {
        var recommendations = await _recommendationService
            .GetPersonalizedRecommendationsAsync(userId, count);
        return Ok(recommendations);
    }

    [HttpGet("services/{serviceId}/similar")]
    public async Task<ActionResult<List<ServiceRecommendation>>> GetSimilar(
        Guid serviceId,
        [FromQuery] int count = 5)
    {
        var similar = await _recommendationService
            .GetSimilarServicesAsync(serviceId, count);
        return Ok(similar);
    }
}
```

### Semantic Search

```csharp
public class SearchController : ControllerBase
{
    private readonly ISemanticSearchService _searchService;

    [HttpGet("search")]
    public async Task<ActionResult<SemanticSearchResult>> Search(
        [FromQuery] string query,
        [FromQuery] string language = "en",
        [FromQuery] int pageNumber = 1)
    {
        var result = await _searchService.SearchServicesAsync(
            query,
            language,
            pageNumber: pageNumber);
        return Ok(result);
    }

    [HttpGet("search/suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestions(
        [FromQuery] string query,
        [FromQuery] string language = "en")
    {
        var suggestions = await _searchService
            .GetSearchSuggestionsAsync(query, language);
        return Ok(suggestions);
    }
}
```

## Implementation Notes

### Chatbot Implementation
- Uses Semantic Kernel chat completion
- Maintains conversation context in memory or database
- Implements multilingual support with language detection
- Provides service suggestions based on conversation
- Includes quick action buttons for common tasks

### Recommendation Algorithm
- Collaborative filtering based on user history
- Content-based filtering using service embeddings
- Location-aware recommendations
- Trending analysis using booking and search data
- Hybrid approach combining multiple signals

### Semantic Search Implementation
- Uses embeddings for semantic similarity
- Implements vector search for efficient retrieval
- Query understanding with intent extraction
- Multilingual support with cross-language search
- Results ranked by relevance score

## Performance Considerations

1. **Caching**: Cache embeddings and frequent queries
2. **Rate Limiting**: Implement rate limiting for API calls
3. **Async Processing**: Use async/await throughout
4. **Batching**: Batch embedding generation
5. **Monitoring**: Track token usage and costs

## Security

1. **API Key Management**: Use Azure Key Vault for secrets
2. **Input Validation**: Sanitize all user inputs
3. **Rate Limiting**: Prevent abuse and control costs
4. **User Context**: Verify user permissions before recommendations
5. **Data Privacy**: Don't send sensitive user data to AI

## Future Enhancements

1. **Voice Integration**: Add speech-to-text and text-to-speech
2. **Image Understanding**: Analyze service images
3. **Sentiment Analysis**: Understand customer satisfaction
4. **Predictive Analytics**: Forecast demand and pricing
5. **Automated Quality Control**: Review service descriptions
6. **Multi-Modal Search**: Search by image or voice

## Cost Optimization

1. Use GPT-3.5-Turbo for most chat interactions
2. Reserve GPT-4 for complex queries
3. Cache embeddings in vector database
4. Implement token limits per user/session
5. Monitor and alert on usage spikes

## Testing

```csharp
// Example unit test
[Fact]
public async Task SendMessage_ReturnsValidResponse()
{
    // Arrange
    var request = new ChatRequest
    {
        SessionId = "test-session",
        Message = "I need a plumber",
        Language = "en"
    };

    // Act
    var response = await _chatService.SendMessageAsync(request);

    // Assert
    Assert.NotNull(response);
    Assert.NotEmpty(response.Message);
    Assert.True(response.ConfidenceScore > 0);
}
```

## Monitoring and Logging

```csharp
_logger.LogInformation(
    "AI Chat Request - SessionId: {SessionId}, Language: {Language}, UserId: {UserId}",
    request.SessionId,
    request.Language,
    request.UserId);

_logger.LogInformation(
    "AI Chat Response - SessionId: {SessionId}, Confidence: {Confidence}, Suggestions: {Count}",
    response.SessionId,
    response.ConfidenceScore,
    response.ServiceSuggestions?.Count ?? 0);
```

## Support

For issues or questions about AI features:
- Check the [Semantic Kernel documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- Review [Azure OpenAI documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- Contact the development team

## License

This implementation uses Microsoft Semantic Kernel (MIT License) and Azure OpenAI Service.
