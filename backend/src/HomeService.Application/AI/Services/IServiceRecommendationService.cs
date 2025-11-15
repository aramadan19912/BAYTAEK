using HomeService.Application.AI.Models;

namespace HomeService.Application.AI.Services;

/// <summary>
/// Service for AI-powered service recommendations
/// </summary>
public interface IServiceRecommendationService
{
    /// <summary>
    /// Gets personalized service recommendations for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="count">Number of recommendations to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recommended services</returns>
    Task<List<ServiceRecommendation>> GetPersonalizedRecommendationsAsync(
        Guid userId,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets similar services based on a given service
    /// </summary>
    /// <param name="serviceId">The source service ID</param>
    /// <param name="count">Number of similar services to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar services</returns>
    Task<List<ServiceRecommendation>> GetSimilarServicesAsync(
        Guid serviceId,
        int count = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trending services based on recent bookings and searches
    /// </summary>
    /// <param name="location">The user's location (optional)</param>
    /// <param name="count">Number of trending services to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trending services</returns>
    Task<List<ServiceRecommendation>> GetTrendingServicesAsync(
        string? location = null,
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service recommendations based on natural language query
    /// </summary>
    /// <param name="query">The user's natural language query</param>
    /// <param name="language">The language of the query (en or ar)</param>
    /// <param name="count">Number of recommendations to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recommended services</returns>
    Task<List<ServiceRecommendation>> GetRecommendationsFromQueryAsync(
        string query,
        string language = "en",
        int count = 10,
        CancellationToken cancellationToken = default);
}
