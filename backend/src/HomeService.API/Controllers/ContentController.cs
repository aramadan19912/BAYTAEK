using HomeService.Application.Queries.Content;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ContentController : BaseApiController
{
    /// <summary>
    /// Get content by type (public access)
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetContentByType(
        ContentType type,
        [FromQuery] string? category,
        [FromQuery] string? tag,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetContentByTypeQuery
        {
            Type = type,
            Category = category,
            Tag = tag,
            IsPublished = true, // Only show published content to public
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get content by slug (public access)
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetContentBySlug(string slug)
    {
        var query = new GetContentBySlugQuery
        {
            Slug = slug
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get FAQs (public access)
    /// </summary>
    [HttpGet("faqs")]
    public async Task<IActionResult> GetFAQs(
        [FromQuery] string? category,
        [FromQuery] string? searchTerm)
    {
        var query = new GetFAQsQuery
        {
            Category = category,
            SearchTerm = searchTerm
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get Terms and Conditions (public access)
    /// </summary>
    [HttpGet("terms")]
    public async Task<IActionResult> GetTermsAndConditions()
    {
        var query = new GetContentByTypeQuery
        {
            Type = ContentType.TermsAndConditions,
            IsPublished = true,
            PageNumber = 1,
            PageSize = 1
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get Privacy Policy (public access)
    /// </summary>
    [HttpGet("privacy")]
    public async Task<IActionResult> GetPrivacyPolicy()
    {
        var query = new GetContentByTypeQuery
        {
            Type = ContentType.PrivacyPolicy,
            IsPublished = true,
            PageNumber = 1,
            PageSize = 1
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get About Us (public access)
    /// </summary>
    [HttpGet("about")]
    public async Task<IActionResult> GetAboutUs()
    {
        var query = new GetContentByTypeQuery
        {
            Type = ContentType.AboutUs,
            IsPublished = true,
            PageNumber = 1,
            PageSize = 1
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get Help Articles (public access)
    /// </summary>
    [HttpGet("help")]
    public async Task<IActionResult> GetHelpArticles(
        [FromQuery] string? category,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetContentByTypeQuery
        {
            Type = ContentType.HelpArticle,
            Category = category,
            IsPublished = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
