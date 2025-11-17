using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Reviews;

public record GetPendingReviewsQuery(
    bool OnlyUnverified = true,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<ReviewDetailDto>>>;

public class GetPendingReviewsQueryHandler
    : IRequestHandler<GetPendingReviewsQuery, Result<PagedResult<ReviewDetailDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPendingReviewsQueryHandler> _logger;

    public GetPendingReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPendingReviewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ReviewDetailDto>>> Handle(
        GetPendingReviewsQuery request,
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
                .AsQueryable();

            // Filter by verification status if requested
            if (request.OnlyUnverified)
            {
                query = query.Where(r => !r.IsVerified);
            }

            // Order by creation date (oldest first for moderation queue)
            query = query.OrderBy(r => r.CreatedAt);

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
            _logger.LogError(ex, "Error getting pending reviews for moderation");
            return Result.Failure<PagedResult<ReviewDetailDto>>(
                "An error occurred while retrieving pending reviews",
                ex.Message);
        }
    }
}
