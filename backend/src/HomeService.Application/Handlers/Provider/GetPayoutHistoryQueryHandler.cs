using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Provider;

public class GetPayoutHistoryQueryHandler : IRequestHandler<GetPayoutHistoryQuery, Result<PayoutHistoryDto>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<Domain.Entities.Payment> _paymentRepository;
    // TODO: Add IRepository<Payout> _payoutRepository when Payout entity is created
    private readonly ILogger<GetPayoutHistoryQueryHandler> _logger;

    private const decimal PlatformFeePercentage = 15m; // 15% platform fee

    public GetPayoutHistoryQueryHandler(
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<Domain.Entities.Payment> paymentRepository,
        ILogger<GetPayoutHistoryQueryHandler> logger)
    {
        _providerRepository = providerRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PayoutHistoryDto>> Handle(GetPayoutHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<PayoutHistoryDto>.Failure("Provider not found");
            }

            // Validate pagination
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100;

            // TODO: Retrieve payouts from database when Payout entity is implemented
            /*
            var allPayouts = await _payoutRepository.FindAsync(
                p => p.ProviderId == request.ProviderId,
                cancellationToken);

            var payoutsList = allPayouts?.OrderByDescending(p => p.RequestedAt).ToList() ?? new List<Payout>();
            var totalCount = payoutsList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedPayouts = payoutsList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PayoutDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Fee = p.Fee,
                    NetAmount = p.NetAmount,
                    PayoutMethod = p.PayoutMethod.ToString().ToLower(),
                    Status = p.Status.ToString().ToLower(),
                    RequestedAt = p.RequestedAt,
                    ProcessedAt = p.ProcessedAt,
                    CompletedAt = p.CompletedAt,
                    EstimatedArrivalDate = p.EstimatedArrivalDate,
                    BankAccountLast4 = p.BankAccountNumber?.Length >= 4 ? p.BankAccountNumber.Substring(p.BankAccountNumber.Length - 4) : null,
                    BankName = p.BankName,
                    FailureReason = p.FailureReason
                })
                .ToList();

            var totalPaidOut = payoutsList.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.NetAmount);
            var pendingPayouts = payoutsList.Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Processing).Sum(p => p.NetAmount);
            var totalFees = payoutsList.Sum(p => p.Fee);
            */

            // For now, return empty list with calculated available balance
            var completedBookings = await _bookingRepository.FindAsync(
                b => b.ProviderId == request.ProviderId && b.Status == BookingStatus.Completed,
                cancellationToken);

            var completedBookingIds = completedBookings?.Select(b => b.Id).ToList() ?? new List<Guid>();

            var completedPayments = await _paymentRepository.FindAsync(
                p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed,
                cancellationToken);

            var totalRevenue = completedPayments?.Sum(p => p.Amount) ?? 0;
            var platformFeeAmount = totalRevenue * (PlatformFeePercentage / 100);
            var availableBalance = totalRevenue - platformFeeAmount;

            var historyDto = new PayoutHistoryDto
            {
                Payouts = new List<PayoutDto>(), // TODO: Replace with actual payouts
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = 0,
                Summary = new PayoutSummaryDto
                {
                    TotalPaidOut = 0, // TODO: Calculate from completed payouts
                    PendingPayouts = 0, // TODO: Calculate from pending payouts
                    AvailableBalance = availableBalance,
                    TotalPayoutCount = 0,
                    TotalFees = 0
                }
            };

            _logger.LogInformation("Retrieved payout history for provider {ProviderId}. Page: {PageNumber}, Size: {PageSize}",
                request.ProviderId, request.PageNumber, request.PageSize);

            return Result<PayoutHistoryDto>.Success(historyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payout history for provider {ProviderId}", request.ProviderId);
            return Result<PayoutHistoryDto>.Failure("An error occurred while retrieving payout history");
        }
    }
}
