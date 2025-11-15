using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Payments;

public record CreatePayoutCommand(
    Guid ProviderId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<PayoutDto>>;

public class CreatePayoutCommandHandler : IRequestHandler<CreatePayoutCommand, Result<PayoutDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePayoutCommandHandler> _logger;
    private const decimal PlatformCommissionRate = 0.15m; // 15% platform fee

    public CreatePayoutCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreatePayoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PayoutDto>> Handle(
        CreatePayoutCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<PayoutDto>("Provider not found");

            // Set date range (default to last 30 days if not specified)
            var endDate = request.EndDate ?? DateTime.UtcNow;
            var startDate = request.StartDate ?? endDate.AddDays(-30);

            // Get completed bookings for provider in date range
            var completedBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Payment)
                .Where(b => b.ProviderId == request.ProviderId
                    && b.Status == BookingStatus.Completed
                    && b.CompletedAt >= startDate
                    && b.CompletedAt <= endDate
                    && b.Payment != null
                    && b.Payment.Status == PaymentStatus.Completed)
                .ToListAsync(cancellationToken);

            if (!completedBookings.Any())
                return Result.Failure<PayoutDto>("No completed bookings found for payout period");

            // Check if bookings already have payouts
            var bookingIds = completedBookings.Select(b => b.Id).ToList();
            var existingPayoutBookings = await _unitOfWork.Repository<PayoutBooking>()
                .GetQueryable()
                .Where(pb => bookingIds.Contains(pb.BookingId))
                .Select(pb => pb.BookingId)
                .ToListAsync(cancellationToken);

            // Filter out bookings that already have payouts
            var eligibleBookings = completedBookings
                .Where(b => !existingPayoutBookings.Contains(b.Id))
                .ToList();

            if (!eligibleBookings.Any())
                return Result.Failure<PayoutDto>("All bookings in this period have already been paid out");

            // Calculate totals
            var totalRevenue = eligibleBookings.Sum(b => b.Payment!.Amount);
            var platformFee = totalRevenue * PlatformCommissionRate;
            var payoutAmount = totalRevenue - platformFee;

            // Create payout record
            var payout = new Payout
            {
                ProviderId = request.ProviderId,
                Amount = payoutAmount,
                Currency = eligibleBookings.First().Currency,
                Status = PayoutStatus.Pending,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalRevenue = totalRevenue,
                PlatformFee = platformFee,
                BookingCount = eligibleBookings.Count
            };

            await _unitOfWork.Repository<Payout>().AddAsync(payout, cancellationToken);

            // Create PayoutBooking records for tracking
            foreach (var booking in eligibleBookings)
            {
                var payoutBooking = new PayoutBooking
                {
                    PayoutId = payout.Id,
                    BookingId = booking.Id,
                    Amount = booking.Payment!.Amount,
                    Commission = booking.Payment.Amount * PlatformCommissionRate
                };

                await _unitOfWork.Repository<PayoutBooking>().AddAsync(payoutBooking, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payout created for provider {ProviderId}. Amount: {Amount}, Bookings: {Count}",
                request.ProviderId, payoutAmount, eligibleBookings.Count);

            var payoutDto = new PayoutDto
            {
                Id = payout.Id,
                ProviderId = payout.ProviderId,
                Amount = payout.Amount,
                Currency = payout.Currency.ToString(),
                Status = payout.Status.ToString(),
                PeriodStart = payout.PeriodStart,
                PeriodEnd = payout.PeriodEnd,
                TotalRevenue = payout.TotalRevenue,
                PlatformFee = payout.PlatformFee,
                BookingCount = payout.BookingCount,
                CreatedAt = payout.CreatedAt
            };

            return Result.Success(payoutDto, "Payout created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payout for provider {ProviderId}", request.ProviderId);
            return Result.Failure<PayoutDto>(
                "An error occurred while creating the payout",
                ex.Message);
        }
    }
}

public class PayoutDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public int BookingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
