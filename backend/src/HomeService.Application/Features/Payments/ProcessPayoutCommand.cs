using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Payments;

public record ProcessPayoutCommand(Guid PayoutId) : IRequest<Result<bool>>;

public class ProcessPayoutCommandHandler : IRequestHandler<ProcessPayoutCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProcessPayoutCommandHandler> _logger;

    public ProcessPayoutCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ProcessPayoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ProcessPayoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get payout with provider details
            var payout = await _unitOfWork.Repository<Payout>()
                .GetQueryable()
                .Include(p => p.Provider)
                    .ThenInclude(sp => sp.User)
                .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

            if (payout == null)
                return Result.Failure<bool>("Payout not found");

            // Validate payout status
            if (payout.Status != PayoutStatus.Pending)
                return Result.Failure<bool>(
                    $"Cannot process payout with status {payout.Status}. Only pending payouts can be processed.");

            // TODO: Integrate with actual payment gateway for bank transfer
            // For now, we'll simulate the process
            // In production, you would:
            // 1. Call Stripe Connect or similar to transfer funds to provider's connected account
            // 2. Verify the transfer was successful
            // 3. Store the transaction reference

            // Simulate transaction ID
            var transactionReference = $"payout_{Guid.NewGuid():N}";

            // Update payout status
            payout.Status = PayoutStatus.Completed;
            payout.ProcessedAt = DateTime.UtcNow;
            payout.TransactionReference = transactionReference;
            _unitOfWork.Repository<Payout>().Update(payout);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send payout notification to provider
            _ = Task.Run(async () =>
            {
                try
                {
                    var titleEn = "Payout Processed";
                    var titleAr = "تمت معالجة الدفع";
                    var messageEn = $"Your payout of {payout.Amount} {payout.Currency} has been processed. " +
                                   $"The funds will arrive in your account within 2-3 business days.";
                    var messageAr = $"تمت معالجة دفعتك البالغة {payout.Amount} {payout.Currency}. " +
                                   $"ستصل الأموال إلى حسابك خلال 2-3 أيام عمل.";

                    var providerId = payout.Provider.UserId;

                    await _notificationService.SendNotificationAsync(
                        providerId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Payment,
                        payout.Id,
                        nameof(Payout),
                        "/provider/payouts",
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending payout notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Payout {PayoutId} processed successfully. Provider: {ProviderId}, Amount: {Amount}",
                request.PayoutId, payout.ProviderId, payout.Amount);

            return Result.Success(true, "Payout processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payout {PayoutId}", request.PayoutId);
            return Result.Failure<bool>(
                "An error occurred while processing the payout",
                ex.Message);
        }
    }
}
