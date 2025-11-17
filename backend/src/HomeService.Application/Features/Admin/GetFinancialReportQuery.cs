using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Admin;

public record GetFinancialReportQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<FinancialReportResponse>>;

public class GetFinancialReportQueryHandler
    : IRequestHandler<GetFinancialReportQuery, Result<FinancialReportResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetFinancialReportQueryHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m;

    public GetFinancialReportQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetFinancialReportQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FinancialReportResponse>> Handle(
        GetFinancialReportQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // Get all payments in period
            var paymentsQuery = _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Include(p => p.Booking)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            var payments = await paymentsQuery.ToListAsync(cancellationToken);

            // Revenue breakdown
            var completedPayments = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .ToList();

            var totalRevenue = completedPayments.Sum(p => p.Amount);
            var platformRevenue = totalRevenue * PlatformCommissionRate;
            var providerRevenue = totalRevenue - platformRevenue;

            // Refund analysis
            var refundedPayments = payments
                .Where(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded)
                .ToList();

            var totalRefunds = refundedPayments.Sum(p => p.RefundAmount ?? 0);
            var refundRate = completedPayments.Count > 0
                ? Math.Round((double)refundedPayments.Count / completedPayments.Count * 100, 2)
                : 0;

            // Payment method breakdown
            var paymentMethodBreakdown = completedPayments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodDto
                {
                    Method = g.Key.ToString(),
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(p => p.Amount)
                .ToList();

            // Payout analysis
            var payoutsQuery = _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            var payouts = await payoutsQuery.ToListAsync(cancellationToken);

            var totalPayouts = payouts.Sum(p => p.Amount);
            var completedPayouts = payouts
                .Where(p => p.Status == PayoutStatus.Completed)
                .Sum(p => p.Amount);
            var pendingPayouts = payouts
                .Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing)
                .Sum(p => p.Amount);

            // Daily financial metrics
            var dailyFinancials = completedPayments
                .GroupBy(p => p.ProcessedAt!.Value.Date)
                .Select(g => new DailyFinancialDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount),
                    PlatformRevenue = g.Sum(p => p.Amount) * PlatformCommissionRate,
                    TransactionCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Calculate platform cash flow
            var cashInflow = totalRevenue;
            var cashOutflow = completedPayouts + totalRefunds;
            var netCashFlow = cashInflow - cashOutflow;

            // Average transaction value
            var averageTransactionValue = completedPayments.Count > 0
                ? Math.Round(totalRevenue / completedPayments.Count, 2)
                : 0;

            var response = new FinancialReportResponse
            {
                // Revenue metrics
                TotalRevenue = totalRevenue,
                PlatformRevenue = platformRevenue,
                ProviderRevenue = providerRevenue,
                TotalTransactions = completedPayments.Count,
                AverageTransactionValue = averageTransactionValue,

                // Refund metrics
                TotalRefunds = totalRefunds,
                RefundCount = refundedPayments.Count,
                RefundRate = refundRate,

                // Payout metrics
                TotalPayouts = totalPayouts,
                CompletedPayouts = completedPayouts,
                PendingPayouts = pendingPayouts,

                // Cash flow
                CashInflow = cashInflow,
                CashOutflow = cashOutflow,
                NetCashFlow = netCashFlow,

                // Breakdowns
                PaymentMethodBreakdown = paymentMethodBreakdown,
                DailyFinancials = dailyFinancials,

                StartDate = startDate,
                EndDate = endDate
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial report");
            return Result.Failure<FinancialReportResponse>(
                "An error occurred while retrieving financial report",
                ex.Message);
        }
    }
}

public class FinancialReportResponse
{
    // Revenue metrics
    public decimal TotalRevenue { get; set; }
    public decimal PlatformRevenue { get; set; }
    public decimal ProviderRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }

    // Refund metrics
    public decimal TotalRefunds { get; set; }
    public int RefundCount { get; set; }
    public double RefundRate { get; set; }

    // Payout metrics
    public decimal TotalPayouts { get; set; }
    public decimal CompletedPayouts { get; set; }
    public decimal PendingPayouts { get; set; }

    // Cash flow
    public decimal CashInflow { get; set; }
    public decimal CashOutflow { get; set; }
    public decimal NetCashFlow { get; set; }

    // Breakdowns
    public List<PaymentMethodDto> PaymentMethodBreakdown { get; set; } = new();
    public List<DailyFinancialDto> DailyFinancials { get; set; } = new();

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class PaymentMethodDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class DailyFinancialDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal PlatformRevenue { get; set; }
    public int TransactionCount { get; set; }
}
