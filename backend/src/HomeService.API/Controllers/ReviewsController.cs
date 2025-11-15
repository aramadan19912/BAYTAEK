using HomeService.Application.Features.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeService.API.Controllers;

[Authorize]
public class ReviewsController : BaseApiController
{
    /// <summary>
    /// Create a review for a completed booking
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new CreateReviewCommand(
            request.BookingId,
            userId,
            request.Rating,
            request.Comment,
            request.ImageUrls,
            request.VideoUrls
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update an existing review (within 48 hours)
    /// </summary>
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new UpdateReviewCommand(
            reviewId,
            userId,
            request.Rating,
            request.Comment,
            request.ImageUrls,
            request.VideoUrls
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new DeleteReviewCommand(reviewId, userId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a specific service
    /// </summary>
    [HttpGet("service/{serviceId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceReviews(
        Guid serviceId,
        [FromQuery] int? minRating = null,
        [FromQuery] bool withImagesOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetServiceReviewsQuery(
            serviceId,
            minRating,
            withImagesOnly,
            pageNumber,
            pageSize
        );

        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a specific provider with rating statistics
    /// </summary>
    [HttpGet("provider/{providerId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderReviews(
        Guid providerId,
        [FromQuery] int? minRating = null,
        [FromQuery] bool withImagesOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProviderReviewsQuery(
            providerId,
            minRating,
            withImagesOnly,
            pageNumber,
            pageSize
        );

        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get my reviews as a customer
    /// </summary>
    [HttpGet("my-reviews")]
    public async Task<IActionResult> GetMyReviews(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetMyReviewsQuery(userId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Provider responds to a review
    /// </summary>
    [HttpPost("{reviewId:guid}/respond")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> RespondToReview(Guid reviewId, [FromBody] RespondToReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new RespondToReviewCommand(reviewId, userId, request.Response);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mark a review as helpful
    /// </summary>
    [HttpPost("{reviewId:guid}/helpful")]
    public async Task<IActionResult> MarkReviewHelpful(Guid reviewId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new MarkReviewHelpfulCommand(reviewId, userId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Admin: Get pending reviews for moderation
    /// </summary>
    [HttpGet("admin/pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetPendingReviews(
        [FromQuery] bool onlyUnverified = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingReviewsQuery(onlyUnverified, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Admin: Moderate a review (approve, reject, hide, show)
    /// </summary>
    [HttpPost("{reviewId:guid}/moderate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ModerateReview(Guid reviewId, [FromBody] ModerateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var command = new ModerateReviewCommand(reviewId, userId, request.Action, request.Reason);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}

// Request DTOs
public record CreateReviewRequest(
    Guid BookingId,
    int Rating,
    string? Comment = null,
    List<string>? ImageUrls = null,
    List<string>? VideoUrls = null
);

public record UpdateReviewRequest(
    int Rating,
    string? Comment = null,
    List<string>? ImageUrls = null,
    List<string>? VideoUrls = null
);

public record RespondToReviewRequest(string Response);

public record ModerateReviewRequest(
    ReviewModerationAction Action,
    string? Reason = null
);
