using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetFinancialAnalyticsQueryHandler : IRequestHandler<GetFinancialAnalyticsQuery, Result<FinancialAnalyticsDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly ILogger<GetFinancialAnalyticsQueryHandler> _logger;

    public GetFinancialAnalyticsQueryHandler(
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        ILogger<GetFinancialAnalyticsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<FinancialAnalyticsDto>> Handle(GetFinancialAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);

            // Filter payments by date range
            var payments = allPayments
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .ToList();

            // Filter bookings by date range
            var bookings = allBookings
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .ToList();

            // Apply region filter if specified
            if (request.Region.HasValue)
            {
                bookings = bookings.Where(b => b.Region == request.Region.Value).ToList();
                var bookingIds = bookings.Select(b => b.Id).ToList();
                payments = payments.Where(p => bookingIds.Contains(p.BookingId)).ToList();
            }

            var analytics = new FinancialAnalyticsDto
            {
                Revenue = CalculateRevenue(payments, bookings, startDate, endDate),
                Commission = CalculateCommission(payments, bookings),
                Payouts = CalculatePayouts(payments, bookings, allUsers),
                Refunds = CalculateRefunds(payments),
                DailyData = CalculateDailyData(payments, bookings, startDate, endDate),
                RegionalBreakdown = CalculateRegionalBreakdown(payments, bookings),
                PaymentMethods = CalculatePaymentMethods(payments)
            };

            return Result.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial analytics");
            return Result.Failure<FinancialAnalyticsDto>("Error retrieving financial analytics", ex.Message);
        }
    }

    private RevenueBreakdown CalculateRevenue(List<Payment> payments, List<HomeService.Domain.Entities.Booking> bookings, DateTime startDate, DateTime endDate)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var totalRevenue = completedPayments.Sum(p => p.Amount);
        var pendingPayments = payments.Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing).ToList();
        var pendingRevenue = pendingPayments.Sum(p => p.Amount);

        // Calculate growth rate compared to previous period
        var periodLength = (endDate - startDate).Days;
        var previousStartDate = startDate.AddDays(-periodLength);
        var previousEndDate = startDate;

        var previousPayments = payments
            .Where(p => p.CreatedAt >= previousStartDate && p.CreatedAt < previousEndDate &&
                       p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        var growthRate = previousPayments > 0 ? ((totalRevenue - previousPayments) / previousPayments) * 100 : 0;

        return new RevenueBreakdown
        {
            TotalRevenue = totalRevenue,
            CompletedBookingsRevenue = totalRevenue,
            PendingRevenue = pendingRevenue,
            AverageTransactionValue = completedPayments.Any() ? totalRevenue / completedPayments.Count : 0,
            TotalTransactions = completedPayments.Count,
            GrowthRate = growthRate
        };
    }

    private CommissionBreakdown CalculateCommission(List<Payment> payments, List<HomeService.Domain.Entities.Booking> bookings)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var commissionRate = 0.18m; // 18%

        var saudiBookingIds = bookings.Where(b => b.Region == Region.SaudiArabia).Select(b => b.Id).ToList();
        var egyptBookingIds = bookings.Where(b => b.Region == Region.Egypt).Select(b => b.Id).ToList();

        var saudiCommission = completedPayments
            .Where(p => saudiBookingIds.Contains(p.BookingId))
            .Sum(p => p.Amount * commissionRate);

        var egyptCommission = completedPayments
            .Where(p => egyptBookingIds.Contains(p.BookingId))
            .Sum(p => p.Amount * commissionRate);

        return new CommissionBreakdown
        {
            TotalCommissionEarned = saudiCommission + egyptCommission,
            CommissionRate = commissionRate,
            CommissionFromSaudi = saudiCommission,
            CommissionFromEgypt = egyptCommission,
            TotalCommissionableBookings = completedPayments.Count
        };
    }

    private PayoutSummary CalculatePayouts(List<Payment> payments, List<HomeService.Domain.Entities.Booking> bookings, IEnumerable<HomeService.Domain.Entities.User> users)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var completedBookings = bookings.Where(b => completedPayments.Any(p => p.BookingId == b.Id) &&
                                                    b.Status == BookingStatus.Completed).ToList();

        var commissionRate = 0.18m;
        var providerPayouts = completedPayments
            .Where(p => completedBookings.Any(b => b.Id == p.BookingId))
            .Sum(p => p.Amount * (1 - commissionRate));

        var providerIds = completedBookings.Where(b => b.ProviderId.HasValue).Select(b => b.ProviderId!.Value).Distinct().ToList();
        var totalProvidersPaid = providerIds.Count;

        // Pending payouts (completed bookings but not yet paid out to providers)
        var pendingPayoutBookings = bookings.Where(b => b.Status == BookingStatus.Completed && b.ProviderId.HasValue).ToList();
        var pendingPayoutProviders = pendingPayoutBookings.Select(b => b.ProviderId!.Value).Distinct().Count();

        return new PayoutSummary
        {
            TotalPayouts = providerPayouts,
            PendingPayouts = pendingPayoutBookings.Sum(b => b.TotalAmount * (1 - commissionRate)),
            ProcessedPayouts = providerPayouts,
            TotalProvidersPaid = totalProvidersPaid,
            ProvidersAwaitingPayout = pendingPayoutProviders
        };
    }

    private RefundSummary CalculateRefunds(List<Payment> payments)
    {
        var refundedPayments = payments.Where(p => p.Status == PaymentStatus.Refunded).ToList();
        var totalRefunded = refundedPayments.Sum(p => p.Amount);
        var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        var refundRate = totalRevenue > 0 ? (totalRefunded / totalRevenue) * 100 : 0;

        // Pending refunds (assuming failed status indicates refund in progress)
        var pendingRefunds = payments.Where(p => p.Status == PaymentStatus.Failed).ToList();

        return new RefundSummary
        {
            TotalRefunded = totalRefunded,
            TotalRefunds = refundedPayments.Count,
            PendingRefunds = pendingRefunds.Sum(p => p.Amount),
            PendingRefundCount = pendingRefunds.Count,
            RefundRate = refundRate
        };
    }

    private List<DailyFinancialData> CalculateDailyData(List<Payment> payments, List<HomeService.Domain.Entities.Booking> bookings, DateTime startDate, DateTime endDate)
    {
        var dailyData = new List<DailyFinancialData>();
        var commissionRate = 0.18m;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayPayments = payments.Where(p => p.ProcessedAt?.Date == date).ToList();
            var completedPayments = dayPayments.Where(p => p.Status == PaymentStatus.Completed).ToList();
            var refundedPayments = dayPayments.Where(p => p.Status == PaymentStatus.Refunded).ToList();

            var revenue = completedPayments.Sum(p => p.Amount);
            var commission = revenue * commissionRate;
            var payouts = revenue * (1 - commissionRate);
            var refunds = refundedPayments.Sum(p => p.Amount);

            dailyData.Add(new DailyFinancialData
            {
                Date = date,
                Revenue = revenue,
                Commission = commission,
                Payouts = payouts,
                Refunds = refunds,
                Transactions = completedPayments.Count
            });
        }

        return dailyData;
    }

    private RegionalFinancials CalculateRegionalBreakdown(List<Payment> payments, List<HomeService.Domain.Entities.Booking> bookings)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var commissionRate = 0.18m;

        var saudiBookings = bookings.Where(b => b.Region == Region.SaudiArabia).ToList();
        var egyptBookings = bookings.Where(b => b.Region == Region.Egypt).ToList();

        var saudiPayments = completedPayments.Where(p => saudiBookings.Any(b => b.Id == p.BookingId)).ToList();
        var egyptPayments = completedPayments.Where(p => egyptBookings.Any(b => b.Id == p.BookingId)).ToList();

        return new RegionalFinancials
        {
            SaudiArabia = new RegionRevenue
            {
                TotalRevenue = saudiPayments.Sum(p => p.Amount),
                Commission = saudiPayments.Sum(p => p.Amount) * commissionRate,
                VatCollected = saudiBookings.Sum(b => b.VatAmount),
                TotalBookings = saudiBookings.Count,
                AverageBookingValue = saudiPayments.Any() ? saudiPayments.Average(p => p.Amount) : 0
            },
            Egypt = new RegionRevenue
            {
                TotalRevenue = egyptPayments.Sum(p => p.Amount),
                Commission = egyptPayments.Sum(p => p.Amount) * commissionRate,
                VatCollected = egyptBookings.Sum(b => b.VatAmount),
                TotalBookings = egyptBookings.Count,
                AverageBookingValue = egyptPayments.Any() ? egyptPayments.Average(p => p.Amount) : 0
            }
        };
    }

    private PaymentMethodBreakdown CalculatePaymentMethods(List<Payment> payments)
    {
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();

        var creditCardPayments = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.CreditCard).ToList();
        var walletPayments = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Wallet).ToList();
        var cashPayments = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Cash).ToList();

        return new PaymentMethodBreakdown
        {
            CreditCardRevenue = creditCardPayments.Sum(p => p.Amount),
            WalletRevenue = walletPayments.Sum(p => p.Amount),
            CashRevenue = cashPayments.Sum(p => p.Amount),
            CreditCardTransactions = creditCardPayments.Count,
            WalletTransactions = walletPayments.Count,
            CashTransactions = cashPayments.Count
        };
    }
}
