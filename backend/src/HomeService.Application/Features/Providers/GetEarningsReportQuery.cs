using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Providers;

public record GetEarningsReportQuery(
    Guid ProviderId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<EarningsTransactionDto>>>;

public class GetEarningsReportQueryHandler
    : IRequestHandler<GetEarningsReportQuery, Result<PagedResult<EarningsTransactionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetEarningsReportQueryHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m;

    public GetEarningsReportQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetEarningsReportQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<EarningsTransactionDto>>> Handle(
        GetEarningsReportQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<PagedResult<EarningsTransactionDto>>("Provider not found");

            // Get completed bookings with payments
            var query = _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Payment)
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .Where(b => b.ProviderId == request.ProviderId &&
                           b.Status == BookingStatus.Completed &&
                           b.Payment != null &&
                           b.Payment.Status == PaymentStatus.Completed);

            if (request.StartDate.HasValue)
                query = query.Where(b => b.CompletedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(b => b.CompletedAt <= request.EndDate.Value);

            // Order by completion date (most recent first)
            query = query.OrderByDescending(b => b.CompletedAt);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var bookings = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var transactions = bookings.Select(b =>
            {
                var amount = b.Payment!.Amount;
                var platformFee = amount * PlatformCommissionRate;
                var netAmount = amount - platformFee;

                return new EarningsTransactionDto
                {
                    BookingId = b.Id,
                    ServiceName = b.Service.NameEn,
                    CustomerName = $"{b.Customer.FirstName} {b.Customer.LastName}",
                    CompletedAt = b.CompletedAt!.Value,
                    GrossAmount = amount,
                    PlatformFee = platformFee,
                    NetAmount = netAmount,
                    PaymentMethod = b.Payment.PaymentMethod.ToString(),
                    TransactionId = b.Payment.TransactionId
                };
            }).ToList();

            var pagedResult = new PagedResult<EarningsTransactionDto>
            {
                Items = transactions,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting earnings report for provider {ProviderId}",
                request.ProviderId);
            return Result.Failure<PagedResult<EarningsTransactionDto>>(
                "An error occurred while retrieving earnings report",
                ex.Message);
        }
    }
}

public class EarningsTransactionDto
{
    public Guid BookingId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}
