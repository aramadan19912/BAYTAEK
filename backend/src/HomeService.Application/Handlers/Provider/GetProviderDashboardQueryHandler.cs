using HomeService.Application.Common;
using HomeService.Application.DTOs.Provider;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Provider;

public class GetProviderDashboardQueryHandler : IRequestHandler<GetProviderDashboardQuery, Result<ProviderDashboardDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly ILogger<GetProviderDashboardQueryHandler> _logger;

    public GetProviderDashboardQueryHandler(
        IRepository<User> userRepository,
        IRepository<Booking> bookingRepository,
        IRepository<Payment> paymentRepository,
        IRepository<Review> reviewRepository,
        ILogger<GetProviderDashboardQueryHandler> logger)
    {
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderDashboardDto>> Handle(GetProviderDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result.Failure<ProviderDashboardDto>("Provider not found");
            }

            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var providerBookings = allBookings.Where(b => b.ProviderId == request.ProviderId).ToList();

            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var allReviews = await _reviewRepository.GetAllAsync(cancellationToken);
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);

            var dashboard = new ProviderDashboardDto
            {
                Today = CalculateTodaySummary(providerBookings, allPayments, allReviews),
                Stats = CalculateQuickStats(providerBookings, allPayments, allReviews),
                IsOnline = !provider.IsDeleted, // Simplified - would use actual online status
                UpcomingJobs = GetUpcomingJobs(providerBookings, allUsers),
                SevenDayEarnings = CalculateSevenDayEarnings(providerBookings, allPayments)
            };

            return Result.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider dashboard for provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderDashboardDto>("Error retrieving dashboard", ex.Message);
        }
    }

    private TodaySummary CalculateTodaySummary(List<Booking> bookings, IEnumerable<Payment> allPayments, IEnumerable<Review> allReviews)
    {
        var today = DateTime.UtcNow.Date;
        var todaysBookings = bookings.Where(b => b.ScheduledDate.Date == today).ToList();

        var completedBookingIds = bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CompletedAt?.Date == today)
            .Select(b => b.Id)
            .ToList();

        var earningsToday = allPayments
            .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * 0.82m); // Provider gets 82% (after 18% commission)

        var providerReviews = allReviews
            .Where(r => bookings.Any(b => b.Id == r.BookingId))
            .ToList();

        var averageRating = providerReviews.Any() ? providerReviews.Average(r => r.Rating) : 0;

        return new TodaySummary
        {
            TodaysBookings = todaysBookings.Count,
            EarningsToday = earningsToday,
            PendingApprovals = bookings.Count(b => b.Status == BookingStatus.Pending),
            CurrentRating = averageRating
        };
    }

    private QuickStats CalculateQuickStats(List<Booking> bookings, IEnumerable<Payment> allPayments, IEnumerable<Review> allReviews)
    {
        var now = DateTime.UtcNow;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var completedBookingIds = completedBookings.Select(b => b.Id).ToList();

        var thisWeekCompletedIds = completedBookings
            .Where(b => b.CompletedAt >= weekStart)
            .Select(b => b.Id)
            .ToList();

        var thisMonthCompletedIds = completedBookings
            .Where(b => b.CompletedAt >= monthStart)
            .Select(b => b.Id)
            .ToList();

        var thisWeekEarnings = allPayments
            .Where(p => thisWeekCompletedIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * 0.82m);

        var thisMonthEarnings = allPayments
            .Where(p => thisMonthCompletedIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount * 0.82m);

        var totalRequests = bookings.Count;
        var acceptedBookings = bookings.Count(b => b.Status != BookingStatus.Cancelled);
        var acceptanceRate = totalRequests > 0 ? (decimal)acceptedBookings / totalRequests * 100 : 0;

        var providerReviews = allReviews
            .Where(r => completedBookingIds.Contains(r.BookingId))
            .ToList();

        var satisfactionScore = providerReviews.Any() ? providerReviews.Average(r => r.Rating) : 0;

        return new QuickStats
        {
            ThisWeekEarnings = thisWeekEarnings,
            ThisMonthEarnings = thisMonthEarnings,
            TotalJobsCompleted = completedBookings.Count,
            AcceptanceRate = acceptanceRate,
            CustomerSatisfactionScore = satisfactionScore
        };
    }

    private List<UpcomingJob> GetUpcomingJobs(List<Booking> bookings, IEnumerable<User> users)
    {
        var upcomingBookings = bookings
            .Where(b => b.ScheduledDate >= DateTime.UtcNow &&
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
            .OrderBy(b => b.ScheduledDate)
            .Take(3)
            .ToList();

        return upcomingBookings.Select(b =>
        {
            var customer = users.FirstOrDefault(u => u.Id == b.CustomerId);

            return new UpcomingJob
            {
                BookingId = b.Id,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                CustomerPhoto = null, // Would come from user profile
                ServiceType = "Service", // Would come from service entity
                ScheduledTime = b.ScheduledDate,
                Location = b.Address,
                Distance = 0, // Would calculate based on provider location
                Earnings = b.ServicePrice * 0.82m
            };
        }).ToList();
    }

    private EarningsChart CalculateSevenDayEarnings(List<Booking> bookings, IEnumerable<Payment> allPayments)
    {
        var today = DateTime.UtcNow.Date;
        var sevenDaysAgo = today.AddDays(-6);
        var fourteenDaysAgo = today.AddDays(-13);

        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var completedBookingIds = completedBookings.Select(b => b.Id).ToList();

        var dailyEarnings = new List<DailyEarning>();
        decimal totalSevenDays = 0;
        decimal previousSevenDays = 0;

        // Last 7 days
        for (int i = 0; i < 7; i++)
        {
            var date = sevenDaysAgo.AddDays(i);
            var dayBookings = completedBookings
                .Where(b => b.CompletedAt?.Date == date)
                .Select(b => b.Id)
                .ToList();

            var dayEarnings = allPayments
                .Where(p => dayBookings.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount * 0.82m);

            totalSevenDays += dayEarnings;

            dailyEarnings.Add(new DailyEarning
            {
                Date = date,
                Amount = dayEarnings,
                JobCount = dayBookings.Count
            });
        }

        // Previous 7 days for comparison
        for (int i = 0; i < 7; i++)
        {
            var date = fourteenDaysAgo.AddDays(i);
            var dayBookings = completedBookings
                .Where(b => b.CompletedAt?.Date == date)
                .Select(b => b.Id)
                .ToList();

            var dayEarnings = allPayments
                .Where(p => dayBookings.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount * 0.82m);

            previousSevenDays += dayEarnings;
        }

        var changePercentage = previousSevenDays > 0
            ? ((totalSevenDays - previousSevenDays) / previousSevenDays) * 100
            : 0;

        return new EarningsChart
        {
            DailyEarnings = dailyEarnings,
            TotalSevenDays = totalSevenDays,
            PreviousSevenDays = previousSevenDays,
            ChangePercentage = changePercentage
        };
    }
}
