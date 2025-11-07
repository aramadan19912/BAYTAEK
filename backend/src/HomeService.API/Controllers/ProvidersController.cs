using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[AllowAnonymous]
public class ProvidersController : BaseApiController
{
    /// <summary>
    /// Search providers with filters
    /// Find service providers by name, rating, region, and more
    /// </summary>
    /// <param name="query">
    /// Filters:
    /// - searchTerm: Search in business name
    /// - region: Filter by region
    /// - categoryId: Providers offering services in this category
    /// - minRating: Minimum average rating (1-5)
    /// - isVerified: Only verified providers
    /// - isOnline: Currently online providers
    /// - isAvailable: Currently available for bookings
    /// - minCompletedBookings: Minimum completed jobs
    /// - sortBy: rating|reviews|bookings|newest|name
    /// - sortOrder: asc|desc
    /// - pageNumber/pageSize: Pagination (max 100 per page)
    /// </param>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProviders([FromQuery] Application.Queries.Provider.SearchProvidersQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider details by ID (public profile)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProviderById(Guid id)
    {
        var query = new Application.Queries.Provider.GetProviderProfileQuery
        {
            ProviderId = id
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}
