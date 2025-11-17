using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Admin;

public record GetPlatformAnalyticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<PlatformAnalyticsResponse>>;

public class GetPlatformAnalyticsQueryHandler
    : IRequestHandler<GetPlatformAnalyticsQuery, Result<PlatformAnalyticsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPlatformAnalyticsQueryHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m;

    public GetPlatformAnalyticsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPlatformAnalyticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PlatformAnalyticsResponse>> Handle(
        GetPlatformAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // User metrics
            var allUsers = await _unitOfWork.Repository<User>()
                .GetAllAsync(cancellationToken);

            var totalUsers = allUsers.Count;
            var activeUsers = allUsers.Count(u => u.IsActive);
            var newUsersInPeriod = allUsers.Count(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);

            // Provider metrics
            var allProviders = await _unitOfWork.Repository<ServiceProvider>()
                .GetAllAsync(cancellationToken);

            var totalProviders = allProviders.Count;
            var verifiedProviders = allProviders.Count(p => p.IsVerified);
            var activeProviders = allProviders.Count(p => p.IsActive);

            // Booking metrics
            var allBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Payment)
                .ToListAsync(cancellationToken);

            var totalBookings = allBookings.Count;
            var bookingsInPeriod = allBookings
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToList();

            var completedBookings = bookingsInPeriod.Count(b => b.Status == BookingStatus.Completed);
            var cancelledBookings = bookingsInPeriod.Count(b => b.Status == BookingStatus.Cancelled);
            var pendingBookings = bookingsInPeriod.Count(b => b.Status == BookingStatus.Pending);

            // Revenue metrics
            var completedWithPayments = bookingsInPeriod
                .Where(b => b.Status == BookingStatus.Completed &&
                           b.Payment != null &&
                           b.Payment.Status == PaymentStatus.Completed)
                .ToList();

            var totalRevenue = completedWithPayments.Sum(b => b.Payment!.Amount);
            var platformRevenue = totalRevenue * PlatformCommissionRate;
            var providerPayouts = totalRevenue - platformRevenue;

            // Review metrics
            var allReviews = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .ToListAsync(cancellationToken);

            var totalReviews = allReviews.Count;
            var averagePlatformRating = allReviews.Any() ? Math.Round(allReviews.Average(r => r.Rating), 2) : 0;

            // Daily trends for the period
            var dailyMetrics = bookingsInPeriod
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new DailyMetricDto
                {
                    Date = g.Key,
                    BookingsCount = g.Count(),
                    CompletedCount = g.Count(b => b.Status == BookingStatus.Completed),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed &&
                                          b.Payment != null &&
                                          b.Payment.Status == PaymentStatus.Completed)
                                .Sum(b => b.Payment!.Amount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Top services
            var topServices = bookingsInPeriod
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => b.ServiceId)
                .Select(g => new
                {
                    ServiceId = g.Key,
                    BookingsCount = g.Count(),
                    Revenue = g.Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
                                .Sum(b => b.Payment!.Amount)
                })
                .OrderByDescending(s => s.BookingsCount)
                .Take(10)
                .ToList();

            var serviceIds = topServices.Select(s => s.ServiceId).ToList();
            var services = await _unitOfWork.Repository<Service>()
                .GetQueryable()
                .Where(s => serviceIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            var topServicesDto = topServices.Select(ts =>
            {
                var service = services.FirstOrDefault(s => s.Id == ts.ServiceId);
                return new TopServiceDto
                {
                    ServiceId = ts.ServiceId,
                    ServiceName = service?.NameEn ?? "Unknown",
                    BookingsCount = ts.BookingsCount,
                    Revenue = ts.Revenue
                };
            }).ToList();

            // Top providers
            var topProviders = bookingsInPeriod
                .Where(b => b.Status == BookingStatus.Completed && b.ProviderId.HasValue)
                .GroupBy(b => b.ProviderId!.Value)
                .Select(g => new
                {
                    ProviderId = g.Key,
                    BookingsCount = g.Count(),
                    Revenue = g.Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
                                .Sum(b => b.Payment!.Amount)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();

            var providerIds = topProviders.Select(p => p.ProviderId).ToList();
            var providersWithUsers = await _unitOfWork.Repository<ServiceProvider>()
                .GetQueryable()
                .Include(p => p.User)
                .Where(p => providerIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var topProvidersDto = topProviders.Select(tp =>
            {
                var provider = providersWithUsers.FirstOrDefault(p => p.Id == tp.ProviderId);
                return new TopProviderDto
                {
                    ProviderId = tp.ProviderId,
                    ProviderName = provider != null ? provider.BusinessName : "Unknown",
                    BookingsCount = tp.BookingsCount,
                    Revenue = tp.Revenue,
                    AverageRating = provider?.AverageRating ?? 0
                };
            }).ToList();

            var response = new PlatformAnalyticsResponse
            {
                // User metrics
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsers = newUsersInPeriod,

                // Provider metrics
                TotalProviders = totalProviders,
                VerifiedProviders = verifiedProviders,
                ActiveProviders = activeProviders,

                // Booking metrics
                TotalBookings = totalBookings,
                BookingsInPeriod = bookingsInPeriod.Count,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                PendingBookings = pendingBookings,
                CompletionRate = bookingsInPeriod.Count > 0
                    ? Math.Round((double)completedBookings / bookingsInPeriod.Count * 100, 2)
                    : 0,

                // Revenue metrics
                TotalRevenue = totalRevenue,
                PlatformRevenue = platformRevenue,
                ProviderPayouts = providerPayouts,

                // Review metrics
                TotalReviews = totalReviews,
                AveragePlatformRating = averagePlatformRating,

                // Trends and rankings
                DailyMetrics = dailyMetrics,
                TopServices = topServicesDto,
                TopProviders = topProvidersDto,

                StartDate = startDate,
                EndDate = endDate
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform analytics");
            return Result.Failure<PlatformAnalyticsResponse>(
                "An error occurred while retrieving platform analytics",
                ex.Message);
        }
    }
}

public class PlatformAnalyticsResponse
{
    // User metrics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }

    // Provider metrics
    public int TotalProviders { get; set; }
    public int VerifiedProviders { get; set; }
    public int ActiveProviders { get; set; }

    // Booking metrics
    public int TotalBookings { get; set; }
    public int BookingsInPeriod { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public double CompletionRate { get; set; }

    // Revenue metrics
    public decimal TotalRevenue { get; set; }
    public decimal PlatformRevenue { get; set; }
    public decimal ProviderPayouts { get; set; }

    // Review metrics
    public int TotalReviews { get; set; }
    public double AveragePlatformRating { get; set; }

    // Trends and rankings
    public List<DailyMetricDto> DailyMetrics { get; set; } = new();
    public List<TopServiceDto> TopServices { get; set; } = new();
    public List<TopProviderDto> TopProviders { get; set; } = new();

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class DailyMetricDto
{
    public DateTime Date { get; set; }
    public int BookingsCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopProviderDto
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
    public double AverageRating { get; set; }
}
