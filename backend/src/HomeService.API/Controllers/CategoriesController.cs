using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

public class CategoriesController : BaseApiController
{
    /// <summary>
    /// Get all service categories
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="includeServiceCount">Include count of services in each category</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool? isActive,
        [FromQuery] bool includeServiceCount = false)
    {
        var query = new Application.Queries.Category.GetCategoriesQuery
        {
            IsActive = isActive,
            IncludeServiceCount = includeServiceCount
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Category.CreateCategoryCommand
        {
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            IconUrl = request.IconUrl,
            IsActive = request.IsActive,
            AdminUserId = adminUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetCategories), new { }, result);
    }

    /// <summary>
    /// Update a category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Category.UpdateCategoryCommand
        {
            CategoryId = id,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            IconUrl = request.IconUrl,
            IsActive = request.IsActive,
            AdminUserId = adminUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a category (Admin only)
    /// Cannot delete if category has services
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Category.DeleteCategoryCommand
        {
            CategoryId = id,
            AdminUserId = adminUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record CreateCategoryRequest(
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    string? IconUrl,
    bool IsActive = true);

public record UpdateCategoryRequest(
    string? NameEn,
    string? NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    string? IconUrl,
    bool? IsActive);
