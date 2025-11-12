using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record GetProviderEarningsQuery(
    Guid ProviderId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<ProviderEarningsResponse>>;

public class GetProviderEarningsQueryHandler
    : IRequestHandler<GetProviderEarningsQuery, Result<ProviderEarningsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderEarningsQueryHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m;

    public GetProviderEarningsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderEarningsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProviderEarningsResponse>> Handle(
        GetProviderEarningsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ProviderEarningsResponse>("Provider not found");

            // Get completed bookings with payments
            var bookingsQuery = _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Payment)
                .Where(b => b.ProviderId == request.ProviderId &&
                           b.Status == BookingStatus.Completed &&
                           b.Payment != null &&
                           b.Payment.Status == PaymentStatus.Completed);

            if (request.StartDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CompletedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CompletedAt <= request.EndDate.Value);

            var bookings = await bookingsQuery.ToListAsync(cancellationToken);

            // Calculate earnings
            var totalRevenue = bookings.Sum(b => b.Payment!.Amount);
            var platformFees = totalRevenue * PlatformCommissionRate;
            var netEarnings = totalRevenue - platformFees;

            // Get payouts
            var payoutsQuery = _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Where(p => p.ProviderId == request.ProviderId);

            if (request.StartDate.HasValue)
                payoutsQuery = payoutsQuery.Where(p => p.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                payoutsQuery = payoutsQuery.Where(p => p.CreatedAt <= request.EndDate.Value);

            var payouts = await payoutsQuery.ToListAsync(cancellationToken);

            var totalPaidOut = payouts
                .Where(p => p.Status == PayoutStatus.Completed)
                .Sum(p => p.Amount);

            var pendingPayouts = payouts
                .Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing)
                .Sum(p => p.Amount);

            // Calculate monthly breakdown
            var monthlyBreakdown = bookings
                .GroupBy(b => new { b.CompletedAt!.Value.Year, b.CompletedAt.Value.Month })
                .Select(g => new MonthlyEarningsDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(b => b.Payment!.Amount),
                    BookingsCount = g.Count(),
                    NetEarnings = g.Sum(b => b.Payment!.Amount) * (1 - PlatformCommissionRate)
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            var response = new ProviderEarningsResponse
            {
                ProviderId = request.ProviderId,
                TotalRevenue = totalRevenue,
                PlatformFees = platformFees,
                NetEarnings = netEarnings,
                TotalPaidOut = totalPaidOut,
                PendingPayouts = pendingPayouts,
                AvailableBalance = netEarnings - totalPaidOut - pendingPayouts,
                TotalBookings = bookings.Count,
                MonthlyBreakdown = monthlyBreakdown,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting earnings for provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderEarningsResponse>(
                "An error occurred while retrieving earnings",
                ex.Message);
        }
    }
}

public class ProviderEarningsResponse
{
    public Guid ProviderId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetEarnings { get; set; }
    public decimal TotalPaidOut { get; set; }
    public decimal PendingPayouts { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TotalBookings { get; set; }
    public List<MonthlyEarningsDto> MonthlyBreakdown { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class MonthlyEarningsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal NetEarnings { get; set; }
    public int BookingsCount { get; set; }
}
