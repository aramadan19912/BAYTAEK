using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class ReviewsController : BaseApiController
{
    /// <summary>
    /// Submit a review for a completed booking
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Review.SubmitReviewCommand
        {
            BookingId = request.BookingId,
            CustomerId = userId,
            Rating = request.Rating,
            Comment = request.Comment,
            ImageUrls = request.ImageUrls,
            IsAnonymous = request.IsAnonymous
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get reviews for a service
    /// </summary>
    [HttpGet("service/{serviceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServiceReviews(
        Guid serviceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? minRating = null,
        [FromQuery] bool withImagesOnly = false)
    {
        var query = new Application.Queries.Review.GetServiceReviewsQuery
        {
            ServiceId = serviceId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            MinRating = minRating,
            WithImagesOnly = withImagesOnly
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Respond to a review (provider only)
    /// </summary>
    [HttpPost("{reviewId}/respond")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> RespondToReview(Guid reviewId, [FromBody] RespondToReviewRequest request)
    {
        var providerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Review.RespondToReviewCommand
        {
            ReviewId = reviewId,
            ProviderId = providerId,
            Response = request.Response
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get provider reviews
    /// </summary>
    [HttpGet("provider/{providerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderReviews(
        Guid providerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? minRating = null)
    {
        // To be implemented
        return Ok(new { message = "Get provider reviews endpoint" });
    }
}

// Request DTOs
public record SubmitReviewRequest(
    Guid BookingId,
    int Rating,
    string? Comment,
    List<string>? ImageUrls,
    bool IsAnonymous);

public record RespondToReviewRequest(string Response);
