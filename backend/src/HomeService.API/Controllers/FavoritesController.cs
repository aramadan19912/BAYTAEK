using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class FavoritesController : BaseApiController
{
    /// <summary>
    /// Get user's favorite services
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new Application.Queries.Favorite.GetUserFavoritesQuery
        {
            UserId = userId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Add service to favorites
    /// </summary>
    [HttpPost("{serviceId}")]
    public async Task<IActionResult> AddFavorite(Guid serviceId)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Favorite.AddFavoriteCommand
        {
            UserId = userId,
            ServiceId = serviceId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Remove service from favorites
    /// </summary>
    [HttpDelete("{serviceId}")]
    public async Task<IActionResult> RemoveFavorite(Guid serviceId)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Favorite.RemoveFavoriteCommand
        {
            UserId = userId,
            ServiceId = serviceId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Check if service is favorited
    /// </summary>
    [HttpGet("check/{serviceId}")]
    public async Task<IActionResult> IsFavorite(Guid serviceId)
    {
        // TODO: Implement when Favorite entity exists
        return Ok(new { isFavorite = false, message = "Favorites system pending implementation" });
    }
}
