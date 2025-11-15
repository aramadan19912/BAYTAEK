using HomeService.Application.Queries.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

public class ServicesController : BaseApiController
{
    /// <summary>
    /// Get all services with optional filters
    /// Search, filter by category/region/price/rating, and sort results
    /// </summary>
    /// <param name="query">
    /// Filters:
    /// - categoryId: Filter by service category
    /// - region: Filter by available region
    /// - searchTerm: Search in service name and description (EN/AR)
    /// - isFeatured: Show only featured services
    /// - minPrice/maxPrice: Price range filter
    /// - minRating: Minimum average rating
    /// - providerId: Services by specific provider
    /// - verifiedProvidersOnly: Only verified providers
    /// - sortBy: price|rating|newest|popular|name
    /// - sortOrder: asc|desc
    /// - pageNumber/pageSize: Pagination
    /// </param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetServices([FromQuery] GetServicesQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceById(Guid id, [FromQuery] string? language = "en")
    {
        var query = new Application.Queries.Service.GetServiceByIdQuery
        {
            ServiceId = id,
            PreferredLanguage = language
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }
}
