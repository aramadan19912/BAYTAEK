using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Providers;

public record GetProviderDashboardQuery(
    Guid ProviderId
) : IRequest<Result<ProviderDashboardResponse>>;

public class GetProviderDashboardQueryHandler
    : IRequestHandler<GetProviderDashboardQuery, Result<ProviderDashboardResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderDashboardQueryHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m;

    public GetProviderDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderDashboardQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProviderDashboardResponse>> Handle(
        GetProviderDashboardQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ProviderDashboardResponse>("Provider not found");

            // Get bookings
            var allBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Payment)
                .Include(b => b.Service)
                .Where(b => b.ProviderId == request.ProviderId)
                .ToListAsync(cancellationToken);

            // Today's metrics
            var today = DateTime.UtcNow.Date;
            var todayBookings = allBookings.Where(b => b.ScheduledAt.Date == today).ToList();

            // This week's metrics (Monday to Sunday)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(7);
            var thisWeekBookings = allBookings
                .Where(b => b.ScheduledAt >= startOfWeek && b.ScheduledAt < endOfWeek)
                .ToList();

            // This month's metrics
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);
            var thisMonthBookings = allBookings
                .Where(b => b.ScheduledAt >= startOfMonth && b.ScheduledAt < endOfMonth)
                .ToList();

            // Earnings this month
            var thisMonthCompletedBookings = thisMonthBookings
                .Where(b => b.Status == BookingStatus.Completed &&
                           b.Payment != null &&
                           b.Payment.Status == PaymentStatus.Completed)
                .ToList();

            var monthlyRevenue = thisMonthCompletedBookings.Sum(b => b.Payment!.Amount);
            var monthlyNetEarnings = monthlyRevenue * (1 - PlatformCommissionRate);

            // Pending actions
            var pendingBookings = allBookings
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.ScheduledAt)
                .Take(10)
                .Select(b => new PendingBookingDto
                {
                    BookingId = b.Id,
                    ServiceName = b.Service.NameEn,
                    ScheduledAt = b.ScheduledAt,
                    Amount = b.TotalAmount,
                    CreatedAt = b.CreatedAt
                })
                .ToList();

            // Upcoming bookings (confirmed)
            var upcomingBookings = allBookings
                .Where(b => b.Status == BookingStatus.Confirmed &&
                           b.ScheduledAt > DateTime.UtcNow)
                .OrderBy(b => b.ScheduledAt)
                .Take(10)
                .Select(b => new UpcomingBookingDto
                {
                    BookingId = b.Id,
                    ServiceName = b.Service.NameEn,
                    ScheduledAt = b.ScheduledAt,
                    Amount = b.TotalAmount
                })
                .ToList();

            // Recent reviews
            var recentReviews = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Service)
                .Where(r => r.ProviderId == request.ProviderId && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentReviewDto
                {
                    ReviewId = r.Id,
                    CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                    ServiceName = r.Booking.Service.NameEn,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    HasResponse = !string.IsNullOrEmpty(r.ProviderResponse)
                })
                .ToListAsync(cancellationToken);

            // Get available balance
            var completedBookings = allBookings
                .Where(b => b.Status == BookingStatus.Completed &&
                           b.Payment != null &&
                           b.Payment.Status == PaymentStatus.Completed)
                .ToList();

            var totalRevenue = completedBookings.Sum(b => b.Payment!.Amount);
            var totalNetEarnings = totalRevenue * (1 - PlatformCommissionRate);

            var payouts = await _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Where(p => p.ProviderId == request.ProviderId)
                .ToListAsync(cancellationToken);

            var totalPaidOut = payouts
                .Where(p => p.Status == PayoutStatus.Completed)
                .Sum(p => p.Amount);

            var pendingPayouts = payouts
                .Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing)
                .Sum(p => p.Amount);

            var availableBalance = totalNetEarnings - totalPaidOut - pendingPayouts;

            var response = new ProviderDashboardResponse
            {
                ProviderId = request.ProviderId,
                ProviderName = provider.BusinessName,
                AverageRating = provider.AverageRating,
                TotalReviews = provider.TotalReviews,
                IsVerified = provider.IsVerified,

                // Today's stats
                TodayBookings = todayBookings.Count,
                TodayCompleted = todayBookings.Count(b => b.Status == BookingStatus.Completed),

                // This week's stats
                WeeklyBookings = thisWeekBookings.Count,
                WeeklyCompleted = thisWeekBookings.Count(b => b.Status == BookingStatus.Completed),

                // This month's stats
                MonthlyBookings = thisMonthBookings.Count,
                MonthlyCompleted = thisMonthBookings.Count(b => b.Status == BookingStatus.Completed),
                MonthlyRevenue = monthlyRevenue,
                MonthlyNetEarnings = monthlyNetEarnings,

                // Financial
                AvailableBalance = availableBalance,
                PendingPayouts = pendingPayouts,

                // Actions needed
                PendingBookingsCount = allBookings.Count(b => b.Status == BookingStatus.Pending),
                UnrespondedReviewsCount = await _unitOfWork.Repository<Review>()
                    .GetQueryable()
                    .CountAsync(r => r.ProviderId == request.ProviderId &&
                                    string.IsNullOrEmpty(r.ProviderResponse), cancellationToken),

                // Lists
                PendingBookings = pendingBookings,
                UpcomingBookings = upcomingBookings,
                RecentReviews = recentReviews
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for provider {ProviderId}",
                request.ProviderId);
            return Result.Failure<ProviderDashboardResponse>(
                "An error occurred while retrieving dashboard data",
                ex.Message);
        }
    }
}

public class ProviderDashboardResponse
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsVerified { get; set; }

    // Today's metrics
    public int TodayBookings { get; set; }
    public int TodayCompleted { get; set; }

    // Weekly metrics
    public int WeeklyBookings { get; set; }
    public int WeeklyCompleted { get; set; }

    // Monthly metrics
    public int MonthlyBookings { get; set; }
    public int MonthlyCompleted { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyNetEarnings { get; set; }

    // Financial
    public decimal AvailableBalance { get; set; }
    public decimal PendingPayouts { get; set; }

    // Actions needed
    public int PendingBookingsCount { get; set; }
    public int UnrespondedReviewsCount { get; set; }

    // Lists
    public List<PendingBookingDto> PendingBookings { get; set; } = new();
    public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
    public List<RecentReviewDto> RecentReviews { get; set; } = new();
}

public class PendingBookingDto
{
    public Guid BookingId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpcomingBookingDto
{
    public Guid BookingId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public decimal Amount { get; set; }
}

public class RecentReviewDto
{
    public Guid ReviewId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasResponse { get; set; }
}
