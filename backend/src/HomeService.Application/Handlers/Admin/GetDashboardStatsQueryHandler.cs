using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<ServiceProvider> providerRepository,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            // Get all data
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var allProviders = await _providerRepository.GetAllAsync(cancellationToken);

            var stats = new DashboardStatsDto
            {
                Users = CalculateUserStats(allUsers, weekStart, monthStart),
                Bookings = CalculateBookingStats(allBookings, today),
                Revenue = CalculateRevenueStats(allPayments, today, weekStart, monthStart),
                Providers = CalculateProviderStats(allProviders),
                System = CalculateSystemStats(allBookings),
                RecentBookings = GetRecentBookings(allBookings),
                TopServices = GetTopServices(allBookings),
                RegionalBreakdown = CalculateRegionalStats(allUsers, allBookings, allPayments, allProviders)
            };

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return Result.Failure<DashboardStatsDto>("Error retrieving dashboard statistics", ex.Message);
        }
    }

    private UserStats CalculateUserStats(IEnumerable<HomeService.Domain.Entities.User> users, DateTime weekStart, DateTime monthStart)
    {
        var customers = users.Where(u => u.Role == UserRole.Customer).ToList();
        var providers = users.Where(u => u.Role == UserRole.ServiceProvider).ToList();
        var newThisWeek = users.Count(u => u.CreatedAt >= weekStart);
        var newThisMonth = users.Count(u => u.CreatedAt >= monthStart);

        return new UserStats
        {
            TotalCustomers = customers.Count,
            TotalProviders = providers.Count,
            ActiveToday = users.Count(u => u.LastLoginAt?.Date == DateTime.UtcNow.Date),
            NewThisWeek = newThisWeek,
            NewThisMonth = newThisMonth,
            GrowthRate = CalculateGrowthRate(users, monthStart)
        };
    }

    private BookingStats CalculateBookingStats(IEnumerable<HomeService.Domain.Entities.Booking> bookings, DateTime today)
    {
        var todayBookings = bookings.Where(b => b.CreatedAt.Date == today).ToList();
        var totalBookings = bookings.Count();

        return new BookingStats
        {
            TotalToday = todayBookings.Count,
            Pending = bookings.Count(b => b.Status == BookingStatus.Pending),
            Confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed),
            InProgress = bookings.Count(b => b.Status == BookingStatus.InProgress),
            Completed = bookings.Count(b => b.Status == BookingStatus.Completed),
            Cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            Disputed = bookings.Count(b => b.Status == BookingStatus.Disputed),
            CompletionRate = totalBookings > 0 ? (decimal)bookings.Count(b => b.Status == BookingStatus.Completed) / totalBookings * 100 : 0,
            CancellationRate = totalBookings > 0 ? (decimal)bookings.Count(b => b.Status == BookingStatus.Cancelled) / totalBookings * 100 : 0
        };
    }

    private RevenueStats CalculateRevenueStats(IEnumerable<Payment> payments, DateTime today, DateTime weekStart, DateTime monthStart)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var yearStart = new DateTime(today.Year, 1, 1);

        var last30Days = new List<DailyRevenue>();
        for (int i = 29; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayRevenue = completedPayments
                .Where(p => p.ProcessedAt?.Date == date)
                .Sum(p => p.Amount);

            last30Days.Add(new DailyRevenue { Date = date, Amount = dayRevenue });
        }

        return new RevenueStats
        {
            Today = completedPayments.Where(p => p.ProcessedAt?.Date == today).Sum(p => p.Amount),
            ThisWeek = completedPayments.Where(p => p.ProcessedAt >= weekStart).Sum(p => p.Amount),
            ThisMonth = completedPayments.Where(p => p.ProcessedAt >= monthStart).Sum(p => p.Amount),
            ThisYear = completedPayments.Where(p => p.ProcessedAt >= yearStart).Sum(p => p.Amount),
            PlatformCommission = completedPayments.Sum(p => p.Amount * 0.18m), // 18% commission
            AverageOrderValue = completedPayments.Any() ? completedPayments.Average(p => p.Amount) : 0,
            Last30Days = last30Days
        };
    }

    private ProviderStats CalculateProviderStats(IEnumerable<ServiceProvider> providers)
    {
        return new ProviderStats
        {
            TotalActive = providers.Count(p => p.IsActive),
            TotalOffline = providers.Count(p => !p.IsActive),
            PendingVerification = providers.Count(p => !p.IsVerified),
            AverageRating = providers.Any() ? providers.Average(p => p.AverageRating) : 0,
            TotalReviews = providers.Sum(p => p.TotalReviews)
        };
    }

    private SystemStats CalculateSystemStats(IEnumerable<HomeService.Domain.Entities.Booking> bookings)
    {
        return new SystemStats
        {
            PendingDisputes = bookings.Count(b => b.Status == BookingStatus.Disputed),
            OpenTickets = 0, // To be implemented with ticket system
            ApiStatus = "Healthy",
            DatabaseStatus = "Healthy"
        };
    }

    private List<RecentBooking> GetRecentBookings(IEnumerable<HomeService.Domain.Entities.Booking> bookings)
    {
        return bookings
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new RecentBooking
            {
                Id = b.Id,
                CustomerName = $"{b.Customer?.FirstName} {b.Customer?.LastName}",
                ServiceName = b.Service?.NameEn ?? "Unknown",
                Status = b.Status.ToString(),
                Amount = b.TotalAmount,
                CreatedAt = b.CreatedAt
            })
            .ToList();
    }

    private List<TopService> GetTopServices(IEnumerable<HomeService.Domain.Entities.Booking> bookings)
    {
        return bookings
            .GroupBy(b => new { b.ServiceId, ServiceName = b.Service?.NameEn })
            .Select(g => new TopService
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName ?? "Unknown",
                BookingCount = g.Count(),
                Revenue = g.Sum(b => b.TotalAmount)
            })
            .OrderByDescending(s => s.BookingCount)
            .Take(10)
            .ToList();
    }

    private RegionalStats CalculateRegionalStats(
        IEnumerable<HomeService.Domain.Entities.User> users,
        IEnumerable<HomeService.Domain.Entities.Booking> bookings,
        IEnumerable<Payment> payments,
        IEnumerable<ServiceProvider> providers)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed);

        return new RegionalStats
        {
            SaudiArabia = new RegionData
            {
                Users = users.Count(u => u.Region == Region.SaudiArabia),
                Bookings = bookings.Count(b => b.Address.Region == Region.SaudiArabia),
                Revenue = completedPayments.Where(p => p.Booking.Address.Region == Region.SaudiArabia).Sum(p => p.Amount),
                ActiveProviders = providers.Count(p => p.User.Region == Region.SaudiArabia && p.IsActive)
            },
            Egypt = new RegionData
            {
                Users = users.Count(u => u.Region == Region.Egypt),
                Bookings = bookings.Count(b => b.Address.Region == Region.Egypt),
                Revenue = completedPayments.Where(p => p.Booking.Address.Region == Region.Egypt).Sum(p => p.Amount),
                ActiveProviders = providers.Count(p => p.User.Region == Region.Egypt && p.IsActive)
            }
        };
    }

    private decimal CalculateGrowthRate(IEnumerable<HomeService.Domain.Entities.User> users, DateTime monthStart)
    {
        var currentMonth = users.Count(u => u.CreatedAt >= monthStart);
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonth = users.Count(u => u.CreatedAt >= lastMonthStart && u.CreatedAt < monthStart);

        if (lastMonth == 0) return 0;
        return ((decimal)currentMonth - lastMonth) / lastMonth * 100;
    }
}
