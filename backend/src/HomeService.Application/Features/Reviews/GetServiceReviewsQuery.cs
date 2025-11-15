using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record GetServiceReviewsQuery(
    Guid ServiceId,
    int? MinRating = null,
    bool WithImagesOnly = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReviewDetailDto>>>;

public class GetServiceReviewsQueryHandler
    : IRequestHandler<GetServiceReviewsQuery, Result<PagedResult<ReviewDetailDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetServiceReviewsQueryHandler> _logger;

    public GetServiceReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetServiceReviewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ReviewDetailDto>>> Handle(
        GetServiceReviewsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get bookings for this service
            var serviceBookingIds = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b => b.ServiceId == request.ServiceId)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            var query = _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                    .ThenInclude(p => p.User)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => serviceBookingIds.Contains(r.BookingId) && r.IsVisible);

            // Apply filters
            if (request.MinRating.HasValue)
                query = query.Where(r => r.Rating >= request.MinRating.Value);

            if (request.WithImagesOnly)
                query = query.Where(r => r.ImageUrls.Length > 0);

            // Order by creation date (most recent first)
            query = query.OrderByDescending(r => r.CreatedAt);

            // Get total count
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

            var pagedResult = new PagedResult<ReviewDetailDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for service {ServiceId}", request.ServiceId);
            return Result.Failure<PagedResult<ReviewDetailDto>>(
                "An error occurred while retrieving service reviews",
                ex.Message);
        }
    }
}

public class ReviewDetailDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAvatarUrl { get; set; }
    public Guid ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> VideoUrls { get; set; } = new();
    public string? ProviderResponse { get; set; }
    public DateTime? ProviderRespondedAt { get; set; }
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
