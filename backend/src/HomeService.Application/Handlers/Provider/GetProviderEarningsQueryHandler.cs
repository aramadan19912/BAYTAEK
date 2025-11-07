using HomeService.Application.Common;
using HomeService.Application.DTOs.Provider;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Provider;

public class GetProviderEarningsQueryHandler : IRequestHandler<GetProviderEarningsQuery, Result<ProviderEarningsDto>>
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ILogger<GetProviderEarningsQueryHandler> _logger;
    private const decimal CommissionRate = 0.18m;
    private const decimal ProviderRate = 0.82m; // 1 - CommissionRate

    public GetProviderEarningsQueryHandler(
        IRepository<Booking> bookingRepository,
        IRepository<Payment> paymentRepository,
        IRepository<Service> serviceRepository,
        IRepository<Category> categoryRepository,
        ILogger<GetProviderEarningsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderEarningsDto>> Handle(GetProviderEarningsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var providerBookings = allBookings
                .Where(b => b.ProviderId == request.ProviderId)
                .ToList();

            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var allServices = await _serviceRepository.GetAllAsync(cancellationToken);
            var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);

            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            var filteredBookings = providerBookings
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToList();

            var earnings = new ProviderEarningsDto
            {
                TotalEarnings = CalculateTotalEarnings(providerBookings, allPayments),
                AvailableBalance = CalculateAvailableBalance(providerBookings, allPayments),
                PendingEarnings = CalculatePendingEarnings(providerBookings, allPayments),
                ThisWeekEarnings = CalculateWeekEarnings(providerBookings, allPayments),
                ThisMonthEarnings = CalculateMonthEarnings(providerBookings, allPayments),
                LastPayout = GetLastPayout(),
                NextPayoutDate = CalculateNextPayoutDate(),
                Breakdown = CalculateBreakdown(filteredBookings, allPayments, allServices, allCategories)
            };

            return Result.Success(earnings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider earnings for provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderEarningsDto>("Error retrieving earnings", ex.Message);
        }
    }

    private decimal CalculateTotalEarnings(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        var completedBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Select(b => b.Id)
            .ToList();

        return allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * ProviderRate);
    }

    private decimal CalculateAvailableBalance(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        // Available balance = completed and paid, but not yet paid out to provider
        // In a real system, would track actual payout status
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var completedBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CompletedAt <= sevenDaysAgo)
            .Select(b => b.Id)
            .ToList();

        return allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * ProviderRate);
    }

    private decimal CalculatePendingEarnings(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        // Pending = completed within last 7 days (holding period)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var pendingBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CompletedAt > sevenDaysAgo)
            .Select(b => b.Id)
            .ToList();

        return allPayments
            .Where(p => pendingBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * ProviderRate);
    }

    private decimal CalculateWeekEarnings(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var completedBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CompletedAt >= weekStart)
            .Select(b => b.Id)
            .ToList();

        return allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * ProviderRate);
    }

    private decimal CalculateMonthEarnings(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var completedBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CompletedAt >= monthStart)
            .Select(b => b.Id)
            .ToList();

        return allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * ProviderRate);
    }

    private PayoutInfo? GetLastPayout()
    {
        // Mock data - would come from payout tracking system
        return new PayoutInfo
        {
            Amount = 2500.00m,
            Date = DateTime.UtcNow.AddDays(-7),
            Status = "Completed"
        };
    }

    private DateTime? CalculateNextPayoutDate()
    {
        // Next Monday for weekly payouts
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7; // Next Monday if today is Monday

        return today.AddDays(daysUntilMonday);
    }

    private EarningsBreakdown CalculateBreakdown(
        List<Booking> bookings,
        IEnumerable<Payment> allPayments,
        IEnumerable<Service> allServices,
        IEnumerable<Category> allCategories)
    {
        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var completedBookingIds = completedBookings.Select(b => b.Id).ToList();

        var payments = allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .ToList();

        var totalEarnings = payments.Sum(p => p.Amount * ProviderRate);

        // By category
        var categoryEarnings = completedBookings
            .GroupBy(b => b.ServiceId)
            .Select(g =>
            {
                var service = allServices.FirstOrDefault(s => s.Id == g.Key);
                var category = service != null ? allCategories.FirstOrDefault(c => c.Id == service.CategoryId) : null;
                var bookingIds = g.Select(b => b.Id).ToList();
                var amount = payments
                    .Where(p => bookingIds.Contains(p.BookingId))
                    .Sum(p => p.Amount * ProviderRate);

                return new CategoryEarnings
                {
                    CategoryName = category?.Name ?? "Other",
                    Amount = amount,
                    JobCount = g.Count(),
                    Percentage = totalEarnings > 0 ? (amount / totalEarnings) * 100 : 0
                };
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Daily earnings
        var dailyEarnings = new List<DailyEarning>();
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-29);

        for (int i = 0; i < 30; i++)
        {
            var date = thirtyDaysAgo.AddDays(i);
            var dayBookingIds = completedBookings
                .Where(b => b.CompletedAt?.Date == date)
                .Select(b => b.Id)
                .ToList();

            var dayEarnings = payments
                .Where(p => dayBookingIds.Contains(p.BookingId))
                .Sum(p => p.Amount * ProviderRate);

            dailyEarnings.Add(new DailyEarning
            {
                Date = date,
                Amount = dayEarnings,
                JobCount = dayBookingIds.Count
            });
        }

        // Peak times
        var peakDays = completedBookings
            .GroupBy(b => b.ScheduledDate.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key.ToString())
            .ToList();

        var peakHours = completedBookings
            .GroupBy(b => b.ScheduledDate.Hour)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"{g.Key}:00")
            .ToList();

        return new EarningsBreakdown
        {
            ByCategory = categoryEarnings,
            Daily = dailyEarnings,
            PeakEarningTimes = new PeakTimes
            {
                PeakDays = peakDays,
                PeakHours = peakHours
            },
            AveragePerJob = completedBookings.Any() ? totalEarnings / completedBookings.Count : 0,
            TotalTips = 0 // Would come from tips tracking if implemented
        };
    }
}
