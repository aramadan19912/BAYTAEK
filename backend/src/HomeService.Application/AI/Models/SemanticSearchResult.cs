namespace HomeService.Application.AI.Models;

/// <summary>
/// Represents the result of a semantic search
/// </summary>
public class SemanticSearchResult
{
    /// <summary>
    /// The original search query
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// The interpreted intent of the query
    /// </summary>
    public string InterpretedIntent { get; set; } = string.Empty;

    /// <summary>
    /// The search results
    /// </summary>
    public List<ServiceSearchResult> Results { get; set; } = new();

    /// <summary>
    /// Total number of results found
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Suggested refinements for the search
    /// </summary>
    public List<string> SuggestedRefinements { get; set; } = new();

    /// <summary>
    /// Related search queries
    /// </summary>
    public List<string> RelatedQueries { get; set; } = new();
}

/// <summary>
/// Represents a single service search result
/// </summary>
public class ServiceSearchResult
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
    /// The service description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// The provider name
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

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
    /// The total number of reviews
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// The relevance score (0-1)
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Highlighted snippets showing why this result matched
    /// </summary>
    public List<string> MatchHighlights { get; set; } = new();

    /// <summary>
    /// Available regions for this service
    /// </summary>
    public List<string> AvailableRegions { get; set; } = new();
}

/// <summary>
/// Search filters for semantic search
/// </summary>
public class SearchFilters
{
    /// <summary>
    /// Filter by category IDs
    /// </summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter by minimum rating
    /// </summary>
    public double? MinRating { get; set; }

    /// <summary>
    /// Filter by region/location
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Filter by provider ID
    /// </summary>
    public Guid? ProviderId { get; set; }

    /// <summary>
    /// Only show available services
    /// </summary>
    public bool OnlyAvailable { get; set; } = true;
}

/// <summary>
/// Represents a help article or FAQ
/// </summary>
public class HelpArticle
{
    /// <summary>
    /// The article ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The article title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The article content/answer
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The category of the help article
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The relevance score (0-1)
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// The number of views
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Was this article helpful?
    /// </summary>
    public int? HelpfulVotes { get; set; }
}
