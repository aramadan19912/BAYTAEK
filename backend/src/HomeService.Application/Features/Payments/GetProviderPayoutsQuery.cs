using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Payments;

public record GetProviderPayoutsQuery(
    Guid ProviderId,
    PayoutStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PayoutDto>>>;

public class GetProviderPayoutsQueryHandler
    : IRequestHandler<GetProviderPayoutsQuery, Result<PagedResult<PayoutDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderPayoutsQueryHandler> _logger;

    public GetProviderPayoutsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderPayoutsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PayoutDto>>> Handle(
        GetProviderPayoutsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Where(p => p.ProviderId == request.ProviderId);

            // Apply filters
            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status.Value);

            // Order by date (most recent first)
            query = query.OrderByDescending(p => p.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var payouts = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var payoutDtos = payouts.Select(p => new PayoutDto
            {
                Id = p.Id,
                ProviderId = p.ProviderId,
                Amount = p.Amount,
                Currency = p.Currency.ToString(),
                Status = p.Status.ToString(),
                PeriodStart = p.PeriodStart ?? DateTime.UtcNow,
                PeriodEnd = p.PeriodEnd ?? DateTime.UtcNow,
                TotalRevenue = p.TotalRevenue ?? 0,
                PlatformFee = p.PlatformFee ?? 0,
                BookingCount = p.BookingCount ?? 0,
                CreatedAt = p.CreatedAt,
                ProcessedAt = p.ProcessedAt
            }).ToList();

            var pagedResult = new PagedResult<PayoutDto>
            {
                Items = payoutDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payouts for provider {ProviderId}", request.ProviderId);
            return Result.Failure<PagedResult<PayoutDto>>(
                "An error occurred while retrieving payouts",
                ex.Message);
        }
    }
}
