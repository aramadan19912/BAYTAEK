using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.ProviderAnalytics;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.ProviderAnalytics;

public class GetProviderAnalyticsQueryHandler : IRequestHandler<GetProviderAnalyticsQuery, Result<ProviderAnalyticsDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly ILogger<GetProviderAnalyticsQueryHandler> _logger;

    private const decimal PlatformFeePercentage = 15m;

    public GetProviderAnalyticsQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        ILogger<GetProviderAnalyticsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderAnalyticsDto>> Handle(GetProviderAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<ProviderAnalyticsDto>.Failure("Provider not found");
            }

            // Get all bookings for the provider in the date range
            var allBookings = await _bookingRepository.FindAsync(
                b => b.ProviderId == request.ProviderId,
                cancellationToken);

            var bookings = allBookings?
                .Where(b => b.CreatedAt >= request.StartDate && b.CreatedAt <= request.EndDate)
                .ToList() ?? new List<Booking>();

            // Get comparison period bookings (same duration before start date)
            var periodDuration = (request.EndDate - request.StartDate).TotalDays;
            var comparisonStartDate = request.StartDate.AddDays(-periodDuration);
            var comparisonBookings = allBookings?
                .Where(b => b.CreatedAt >= comparisonStartDate && b.CreatedAt < request.StartDate)
                .ToList() ?? new List<Booking>();

            // Get payments
            var bookingIds = bookings.Select(b => b.Id).ToList();
            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var payments = allPayments
                .Where(p => bookingIds.Contains(p.BookingId))
                .ToList();

            // Get reviews
            var allReviews = await _reviewRepository.GetAllAsync(cancellationToken);
            var reviews = allReviews
                .Where(r => bookingIds.Contains(r.BookingId))
                .ToList();

            // Calculate earnings analytics
            var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();

            var totalRevenue = completedPayments.Sum(p => p.Amount);
            var platformFees = totalRevenue * (PlatformFeePercentage / 100);
            var netEarnings = totalRevenue - platformFees;
            var averageBookingValue = completedBookings.Any() ? totalRevenue / completedBookings.Count : 0;

            // Projected monthly earnings (extrapolate based on period)
            var daysInPeriod = (request.EndDate - request.StartDate).TotalDays;
            var projectedMonthlyEarnings = daysInPeriod > 0 ? (netEarnings / (decimal)daysInPeriod) * 30 : 0;

            // Comparison period revenue
            var comparisonBookingIds = comparisonBookings.Select(b => b.Id).ToList();
            var comparisonPayments = allPayments
                .Where(p => comparisonBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
                .ToList();
            var comparisonRevenue = comparisonPayments.Sum(p => p.Amount);
            var revenueGrowth = comparisonRevenue > 0
                ? ((totalRevenue - comparisonRevenue) / comparisonRevenue) * 100
                : 0;

            var earnings = new EarningsAnalyticsDto
            {
                TotalRevenue = totalRevenue,
                PlatformFees = platformFees,
                NetEarnings = netEarnings,
                AverageBookingValue = averageBookingValue,
                ProjectedMonthlyEarnings = projectedMonthlyEarnings,
                CompletedBookingsRevenue = totalRevenue,
                PendingPayoutsAmount = netEarnings, // Simplified
                PaidOutAmount = 0, // TODO: Calculate from Payout entity when implemented
                RevenueGrowthPercentage = Math.Round(revenueGrowth, 2),
                ComparisonPeriodRevenue = comparisonRevenue
            };

            // Calculate booking analytics
            var totalBookings = bookings.Count;
            var completedCount = bookings.Count(b => b.Status == BookingStatus.Completed);
            var cancelledCount = bookings.Count(b => b.Status == BookingStatus.Cancelled);
            var inProgressCount = bookings.Count(b => b.Status == BookingStatus.InProgress);
            var pendingCount = bookings.Count(b => b.Status == BookingStatus.Pending);

            var completionRate = totalBookings > 0 ? (decimal)completedCount / totalBookings * 100 : 0;
            var cancellationRate = totalBookings > 0 ? (decimal)cancelledCount / totalBookings * 100 : 0;

            // Average response time (time from created to confirmed)
            var confirmedBookings = bookings.Where(b => b.ConfirmedAt.HasValue).ToList();
            var averageResponseTime = confirmedBookings.Any()
                ? confirmedBookings.Average(b => (b.ConfirmedAt!.Value - b.CreatedAt).TotalHours)
                : 0;

            var comparisonBookingsCount = comparisonBookings.Count;
            var bookingGrowth = totalBookings - comparisonBookingsCount;
            var bookingGrowthPercentage = comparisonBookingsCount > 0
                ? ((decimal)bookingGrowth / comparisonBookingsCount) * 100
                : 0;

            var bookingAnalytics = new BookingAnalyticsDto
            {
                TotalBookings = totalBookings,
                CompletedBookings = completedCount,
                CancelledBookings = cancelledCount,
                InProgressBookings = inProgressCount,
                PendingBookings = pendingCount,
                CompletionRate = Math.Round(completionRate, 2),
                CancellationRate = Math.Round(cancellationRate, 2),
                AverageResponseTime = Math.Round((decimal)averageResponseTime, 2),
                BookingGrowthCount = bookingGrowth,
                BookingGrowthPercentage = Math.Round(bookingGrowthPercentage, 2),
                ComparisonPeriodBookings = comparisonBookingsCount
            };

            // Calculate performance analytics
            var ratingDistribution = reviews.GroupBy(r => r.Rating).ToDictionary(g => g.Key, g => g.Count());
            var fiveStarCount = ratingDistribution.GetValueOrDefault(5, 0);
            var fourStarCount = ratingDistribution.GetValueOrDefault(4, 0);
            var threeStarCount = ratingDistribution.GetValueOrDefault(3, 0);
            var twoStarCount = ratingDistribution.GetValueOrDefault(2, 0);
            var oneStarCount = ratingDistribution.GetValueOrDefault(1, 0);

            var averageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
            var responseRate = totalBookings > 0 ? (decimal)confirmedBookings.Count / totalBookings * 100 : 0;

            // On-time completion (completed within scheduled date)
            var onTimeCompletions = completedBookings.Count(b =>
                b.CompletedAt.HasValue &&
                b.CompletedAt.Value.Date <= b.ScheduledDateTime.Date);
            var onTimeRate = completedBookings.Any() ? (decimal)onTimeCompletions / completedBookings.Count * 100 : 0;

            // Customer satisfaction (% of 4-5 star reviews)
            var satisfactionScore = reviews.Any()
                ? (decimal)(fiveStarCount + fourStarCount) / reviews.Count * 100
                : 0;

            // Comparison period rating
            var comparisonReviews = allReviews
                .Where(r => comparisonBookingIds.Contains(r.BookingId))
                .ToList();
            var comparisonRating = comparisonReviews.Any() ? (decimal)comparisonReviews.Average(r => r.Rating) : 0;
            var ratingGrowth = averageRating - comparisonRating;

            var performance = new PerformanceAnalyticsDto
            {
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = reviews.Count,
                FiveStarReviews = fiveStarCount,
                FourStarReviews = fourStarCount,
                ThreeStarReviews = threeStarCount,
                TwoStarReviews = twoStarCount,
                OneStarReviews = oneStarCount,
                ResponseRate = Math.Round(responseRate, 2),
                OnTimeCompletionRate = Math.Round(onTimeRate, 2),
                CustomerSatisfactionScore = Math.Round(satisfactionScore, 2),
                RatingGrowth = Math.Round(ratingGrowth, 2),
                ComparisonPeriodRating = Math.Round(comparisonRating, 2)
            };

            // Calculate customer insights
            var uniqueCustomerIds = bookings.Select(b => b.CustomerId).Distinct().ToList();
            var allCustomerBookings = allBookings?.Where(b => b.CreatedAt < request.StartDate).ToList() ?? new List<Booking>();
            var existingCustomerIds = allCustomerBookings.Select(b => b.CustomerId).Distinct().ToHashSet();

            var newCustomers = uniqueCustomerIds.Count(c => !existingCustomerIds.Contains(c));
            var returningCustomers = uniqueCustomerIds.Count(c => existingCustomerIds.Contains(c));
            var retentionRate = uniqueCustomerIds.Any() ? (decimal)returningCustomers / uniqueCustomerIds.Count * 100 : 0;

            // Top customers
            var customerBookingCounts = bookings
                .GroupBy(b => b.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    BookingsCount = g.Count(),
                    BookingIds = g.Select(b => b.Id).ToList()
                })
                .OrderByDescending(x => x.BookingsCount)
                .Take(5)
                .ToList();

            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var topCustomers = customerBookingCounts.Select(c =>
            {
                var user = allUsers.FirstOrDefault(u => u.Id == c.CustomerId);
                var customerPayments = payments.Where(p => c.BookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed).ToList();
                var totalSpent = customerPayments.Sum(p => p.Amount);

                return new TopCustomerDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = user?.FirstName ?? "Unknown Customer",
                    BookingsCount = c.BookingsCount,
                    TotalSpent = totalSpent
                };
            }).ToList();

            var averageCustomerLifetimeValue = uniqueCustomerIds.Any() ? totalRevenue / uniqueCustomerIds.Count : 0;

            var customers = new CustomerInsightsDto
            {
                TotalCustomers = uniqueCustomerIds.Count,
                NewCustomers = newCustomers,
                ReturningCustomers = returningCustomers,
                RetentionRate = Math.Round(retentionRate, 2),
                AverageCustomerLifetimeValue = Math.Round(averageCustomerLifetimeValue, 2),
                TopCustomers = topCustomers
            };

            // Top services
            var allServices = await _serviceRepository.GetAllAsync(cancellationToken);
            var serviceBookings = bookings
                .GroupBy(b => b.ServiceId)
                .Select(g => new
                {
                    ServiceId = g.Key,
                    BookingsCount = g.Count(),
                    Revenue = payments.Where(p => g.Select(b => b.Id).Contains(p.BookingId) && p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                    BookingIds = g.Select(b => b.Id).ToList()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var topServices = serviceBookings.Select(s =>
            {
                var service = allServices.FirstOrDefault(svc => svc.Id == s.ServiceId);
                var serviceReviews = reviews.Where(r => s.BookingIds.Contains(r.BookingId)).ToList();
                var avgRating = serviceReviews.Any() ? (decimal)serviceReviews.Average(r => r.Rating) : 0;

                return new ServicePerformanceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceNameEn = service?.NameEn ?? "Unknown Service",
                    ServiceNameAr = service?.NameAr ?? "خدمة غير معروفة",
                    BookingsCount = s.BookingsCount,
                    Revenue = s.Revenue,
                    AverageRating = Math.Round(avgRating, 2),
                    ReviewsCount = serviceReviews.Count
                };
            }).ToList();

            // Daily trends
            var dailyGroups = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .ToList();

            var dailyTrends = dailyGroups.Select(g =>
            {
                var dayBookingIds = g.Select(b => b.Id).ToList();
                var dayPayments = payments.Where(p => dayBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed).ToList();
                var dayReviews = reviews.Where(r => dayBookingIds.Contains(r.BookingId)).ToList();

                return new DailyTrendDto
                {
                    Date = g.Key,
                    BookingsCount = g.Count(),
                    Revenue = dayPayments.Sum(p => p.Amount),
                    AverageRating = dayReviews.Any() ? Math.Round((decimal)dayReviews.Average(r => r.Rating), 2) : 0
                };
            }).ToList();

            var result = new ProviderAnalyticsDto
            {
                Earnings = earnings,
                Bookings = bookingAnalytics,
                Performance = performance,
                Customers = customers,
                TopServices = topServices,
                DailyTrends = dailyTrends
            };

            return Result<ProviderAnalyticsDto>.Success(result, "Provider analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider analytics for provider {ProviderId}", request.ProviderId);
            return Result<ProviderAnalyticsDto>.Failure("An error occurred while retrieving provider analytics");
        }
    }
}
