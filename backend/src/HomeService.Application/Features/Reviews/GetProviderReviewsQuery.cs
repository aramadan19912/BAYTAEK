using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Reviews;

public record GetProviderReviewsQuery(
    Guid ProviderId,
    int? MinRating = null,
    bool WithImagesOnly = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<ProviderReviewsResponse>>;

public class GetProviderReviewsQueryHandler
    : IRequestHandler<GetProviderReviewsQuery, Result<ProviderReviewsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderReviewsQueryHandler> _logger;

    public GetProviderReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderReviewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProviderReviewsResponse>> Handle(
        GetProviderReviewsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                    .ThenInclude(p => p.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ProviderId == request.ProviderId && r.IsVisible);

            // Apply filters
            if (request.MinRating.HasValue)
                query = query.Where(r => r.Rating >= request.MinRating.Value);

            if (request.WithImagesOnly)
                query = query.Where(r => r.ImageUrls.Length > 0);

            // Get rating statistics before pagination
            var allReviews = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.ProviderId == request.ProviderId && r.IsVisible)
                .ToListAsync(cancellationToken);

            var ratingStats = new RatingStatistics
            {
                TotalReviews = allReviews.Count,
                AverageRating = allReviews.Any() ? Math.Round(allReviews.Average(r => r.Rating), 2) : 0,
                FiveStarCount = allReviews.Count(r => r.Rating == 5),
                FourStarCount = allReviews.Count(r => r.Rating == 4),
                ThreeStarCount = allReviews.Count(r => r.Rating == 3),
                TwoStarCount = allReviews.Count(r => r.Rating == 2),
                OneStarCount = allReviews.Count(r => r.Rating == 1)
            };

            // Order by creation date (most recent first)
            query = query.OrderByDescending(r => r.CreatedAt);

            // Get total count for filtered query
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var reviews = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var reviewDtos = reviews.Select(r => new ReviewDetailDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                CustomerId = r.CustomerId,
                CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                CustomerAvatarUrl = r.Customer.ProfilePhotoUrl,
                ProviderId = r.ProviderId,
                ProviderName = $"{r.Provider.User.FirstName} {r.Provider.User.LastName}",
                ServiceName = r.Booking.Service.NameEn,
                Rating = r.Rating,
                Comment = r.Comment,
                ImageUrls = r.ImageUrls.ToList(),
                VideoUrls = r.VideoUrls.ToList(),
                ProviderResponse = r.ProviderResponse,
                ProviderRespondedAt = r.ProviderRespondedAt,
                IsVerified = r.IsVerified,
                HelpfulCount = r.HelpfulCount,
                CreatedAt = r.CreatedAt
            }).ToList();

            var response = new ProviderReviewsResponse
            {
                RatingStatistics = ratingStats,
                Reviews = new PagedResult<ReviewDetailDto>
                {
                    Items = reviewDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderReviewsResponse>(
                "An error occurred while retrieving provider reviews",
                ex.Message);
        }
    }
}

public class ProviderReviewsResponse
{
    public RatingStatistics RatingStatistics { get; set; } = new();
    public PagedResult<ReviewDetailDto> Reviews { get; set; } = new();
}

public class RatingStatistics
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }

    public double FiveStarPercentage => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
    public double FourStarPercentage => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
    public double ThreeStarPercentage => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
    public double TwoStarPercentage => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
    public double OneStarPercentage => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;
}
