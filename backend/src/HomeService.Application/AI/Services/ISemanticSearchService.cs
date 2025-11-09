using HomeService.Application.AI.Models;

namespace HomeService.Application.AI.Services;

/// <summary>
/// Service for AI-powered semantic search
/// </summary>
public interface ISemanticSearchService
{
    /// <summary>
    /// Performs semantic search for services based on natural language query
    /// </summary>
    /// <param name="query">The natural language search query</param>
    /// <param name="language">The language of the query (en or ar)</param>
    /// <param name="filters">Optional search filters</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with relevance scores</returns>
    Task<SemanticSearchResult> SearchServicesAsync(
        string query,
        string language = "en",
        SearchFilters? filters = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets search suggestions based on partial query
    /// </summary>
    /// <param name="partialQuery">The partial search query</param>
    /// <param name="language">The language of the query (en or ar)</param>
    /// <param name="count">Number of suggestions to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of search suggestions</returns>
    Task<List<string>> GetSearchSuggestionsAsync(
        string partialQuery,
        string language = "en",
        int count = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs semantic search for FAQ/help content
    /// </summary>
    /// <param name="question">The user's question</param>
    /// <param name="language">The language of the question (en or ar)</param>
    /// <param name="count">Number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relevant FAQ/help articles</returns>
    Task<List<HelpArticle>> SearchHelpContentAsync(
        string question,
        string language = "en",
        int count = 5,
        CancellationToken cancellationToken = default);
}
