using HomeService.Application.Queries.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

public class ServicesController : BaseApiController
{
    /// <summary>
    /// Get all services with optional filters
    /// </summary>
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
