using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.BackgroundJobs;

public class PayoutProcessingJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PayoutProcessingJob> _logger;
    private const decimal PlatformCommissionRate = 0.15m;
    private const int MinimumPayoutAmount = 50; // Minimum $50 for payout

    public PayoutProcessingJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<PayoutProcessingJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ProcessScheduledPayoutsAsync()
    {
        try
        {
            _logger.LogInformation("Starting scheduled payout processing job");

            // Get pending payouts that are ready to be processed
            var pendingPayouts = await _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Include(p => p.Provider)
                    .ThenInclude(p => p.User)
                .Include(p => p.PayoutBookings)
                    .ThenInclude(pb => pb.Booking)
                .Where(p => p.Status == PayoutStatus.Pending &&
                           p.CreatedAt <= DateTime.UtcNow.AddDays(-7)) // Process after 7 days
                .ToListAsync();

            _logger.LogInformation("Found {Count} pending payouts to process", pendingPayouts.Count);

            foreach (var payout in pendingPayouts)
            {
                try
                {
                    // Update status to processing
                    payout.Status = PayoutStatus.Processing;
                    payout.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Payout>().Update(payout);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    // Here you would integrate with actual payment gateway (Stripe, PayPal, etc.)
                    // For now, we'll simulate successful processing
                    await Task.Delay(100); // Simulate processing time

                    // Mark as completed
                    payout.Status = PayoutStatus.Completed;
                    payout.ProcessedAt = DateTime.UtcNow;
                    payout.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Payout>().Update(payout);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    // Send notification to provider
                    var titleEn = "Payout Processed";
                    var titleAr = "تم معالجة الدفعة";
                    var messageEn = $"Your payout of ${payout.Amount:F2} has been processed successfully. Transaction ID: {payout.TransactionId}";
                    var messageAr = $"تمت معالجة دفعتك بمبلغ ${payout.Amount:F2} بنجاح. رقم المعاملة: {payout.TransactionId}";

                    await _notificationService.SendNotificationAsync(
                        payout.Provider.UserId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Payment,
                        payout.Id,
                        nameof(Payout),
                        $"/provider/payouts/{payout.Id}",
                        CancellationToken.None);

                    _logger.LogInformation("Payout {PayoutId} processed successfully for provider {ProviderId}",
                        payout.Id, payout.ProviderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payout {PayoutId}", payout.Id);

                    // Mark as failed
                    payout.Status = PayoutStatus.Failed;
                    payout.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Payout>().Update(payout);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    // Notify provider of failure
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            payout.Provider.UserId,
                            "Payout Failed",
                            "فشلت الدفعة",
                            $"Your payout of ${payout.Amount:F2} failed to process. Please contact support.",
                            $"فشلت معالجة دفعتك بمبلغ ${payout.Amount:F2}. يرجى الاتصال بالدعم.",
                            NotificationCategory.Payment,
                            payout.Id,
                            nameof(Payout),
                            $"/provider/payouts/{payout.Id}",
                            CancellationToken.None);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Error sending payout failure notification");
                    }
                }
            }

            _logger.LogInformation("Scheduled payout processing job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in scheduled payout processing job");
            throw;
        }
    }

    public async Task AutoCreatePayoutsForProvidersAsync()
    {
        try
        {
            _logger.LogInformation("Starting auto-create payouts job");

            // Get all active providers
            var providers = await _unitOfWork.Repository<ServiceProvider>()
                .GetQueryable()
                .Where(p => p.IsActive)
                .ToListAsync();

            foreach (var provider in providers)
            {
                try
                {
                    // Get completed bookings that haven't been paid out yet
                    var eligibleBookings = await _unitOfWork.Repository<Booking>()
                        .GetQueryable()
                        .Include(b => b.Payment)
                        .Where(b => b.ProviderId == provider.Id &&
                                   b.Status == BookingStatus.Completed &&
                                   b.Payment != null &&
                                   b.Payment.Status == PaymentStatus.Completed &&
                                   b.CompletedAt.HasValue &&
                                   b.CompletedAt.Value <= DateTime.UtcNow.AddDays(-7) && // 7 days after completion
                                   !_unitOfWork.Repository<PayoutBooking>()
                                       .GetQueryable()
                                       .Any(pb => pb.BookingId == b.Id))
                        .ToListAsync();

                    if (!eligibleBookings.Any())
                        continue;

                    var totalRevenue = eligibleBookings.Sum(b => b.Payment!.Amount);
                    var platformFee = totalRevenue * PlatformCommissionRate;
                    var payoutAmount = totalRevenue - platformFee;

                    // Check minimum payout amount
                    if (payoutAmount < MinimumPayoutAmount)
                        continue;

                    // Create payout
                    var payout = new Payout
                    {
                        ProviderId = provider.Id,
                        Amount = payoutAmount,
                        Status = PayoutStatus.Pending,
                        TransactionId = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    await _unitOfWork.Repository<Payout>().AddAsync(payout, CancellationToken.None);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    // Link bookings to payout
                    foreach (var booking in eligibleBookings)
                    {
                        var payoutBooking = new PayoutBooking
                        {
                            PayoutId = payout.Id,
                            BookingId = booking.Id
                        };
                        await _unitOfWork.Repository<PayoutBooking>().AddAsync(payoutBooking, CancellationToken.None);
                    }

                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    _logger.LogInformation("Auto-created payout {PayoutId} for provider {ProviderId} with amount ${Amount}",
                        payout.Id, provider.Id, payoutAmount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auto-creating payout for provider {ProviderId}", provider.Id);
                }
            }

            _logger.LogInformation("Auto-create payouts job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-create payouts job");
            throw;
        }
    }
}
