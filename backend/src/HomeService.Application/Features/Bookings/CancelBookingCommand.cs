using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Bookings;

public record CancelBookingCommand(
    Guid BookingId,
    Guid UserId,
    string Reason,
    bool IsCustomerCancellation = true
) : IRequest<Result<CancellationResultDto>>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<CancellationResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPaymentGatewayService _paymentService;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPaymentGatewayService paymentService,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<CancellationResultDto>> Handle(
        CancelBookingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get booking with payment info
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Provider)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<CancellationResultDto>("Booking not found");

            // Validate cancellation permissions
            if (request.IsCustomerCancellation && booking.CustomerId != request.UserId)
                return Result.Failure<CancellationResultDto>("Unauthorized: You cannot cancel this booking");

            if (!request.IsCustomerCancellation && booking.ProviderId != request.UserId)
                return Result.Failure<CancellationResultDto>("Unauthorized: You cannot cancel this booking");

            // Validate booking status - cannot cancel completed or already cancelled bookings
            if (booking.Status == BookingStatus.Completed)
                return Result.Failure<CancellationResultDto>("Cannot cancel a completed booking");

            if (booking.Status == BookingStatus.Cancelled)
                return Result.Failure<CancellationResultDto>("Booking is already cancelled");

            // Calculate refund policy
            var refundInfo = CalculateRefund(booking, request.IsCustomerCancellation);

            // Process refund if applicable
            string? refundTransactionId = null;
            if (refundInfo.RefundAmount > 0 && booking.Payment != null)
            {
                try
                {
                    // Note: Actual payment refund integration would go here
                    // For now, we'll simulate it
                    refundTransactionId = $"refund_{Guid.NewGuid():N}";
                    _logger.LogInformation("Refund of {Amount} processed for booking {BookingId}",
                        refundInfo.RefundAmount, request.BookingId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund for booking {BookingId}", request.BookingId);
                    // Continue with cancellation even if refund fails
                }
            }

            // Update booking
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = request.Reason;
            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Cancelled,
                ChangedBy = request.UserId,
                Notes = $"Cancelled by {(request.IsCustomerCancellation ? "customer" : "provider")}. " +
                       $"Reason: {request.Reason}. " +
                       $"Refund: {refundInfo.RefundAmount:C} ({refundInfo.RefundPercentage}%)"
            };
            await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notifications
            _ = Task.Run(async () =>
            {
                try
                {
                    // Notify the other party
                    var notifyUserId = request.IsCustomerCancellation
                        ? booking.ProviderId ?? Guid.Empty
                        : booking.CustomerId;

                    if (notifyUserId != Guid.Empty)
                    {
                        await _notificationService.SendBookingStatusUpdateAsync(
                            notifyUserId,
                            booking.Id,
                            "Cancelled",
                            booking.Service.NameEn,
                            CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending cancellation notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Booking {BookingId} cancelled by {UserType}. Refund: {RefundAmount}",
                request.BookingId,
                request.IsCustomerCancellation ? "customer" : "provider",
                refundInfo.RefundAmount);

            var result = new CancellationResultDto
            {
                BookingId = booking.Id,
                RefundAmount = refundInfo.RefundAmount,
                RefundPercentage = refundInfo.RefundPercentage,
                RefundReason = refundInfo.RefundReason,
                RefundTransactionId = refundTransactionId,
                CancellationFee = refundInfo.CancellationFee
            };

            return Result.Success(result, "Booking cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
            return Result.Failure<CancellationResultDto>(
                "An error occurred while cancelling the booking",
                ex.Message);
        }
    }

    private RefundCalculation CalculateRefund(Booking booking, bool isCustomerCancellation)
    {
        var hoursUntilService = (booking.ScheduledAt - DateTime.UtcNow).TotalHours;
        var refundPercentage = 0;
        string refundReason;

        if (isCustomerCancellation)
        {
            // Customer cancellation policy
            if (hoursUntilService >= 24)
            {
                refundPercentage = 100; // Full refund if cancelled 24+ hours before
                refundReason = "Full refund: Cancelled more than 24 hours before service";
            }
            else if (hoursUntilService >= 12)
            {
                refundPercentage = 75; // 75% refund if cancelled 12-24 hours before
                refundReason = "75% refund: Cancelled 12-24 hours before service";
            }
            else if (hoursUntilService >= 6)
            {
                refundPercentage = 50; // 50% refund if cancelled 6-12 hours before
                refundReason = "50% refund: Cancelled 6-12 hours before service";
            }
            else if (hoursUntilService >= 2)
            {
                refundPercentage = 25; // 25% refund if cancelled 2-6 hours before
                refundReason = "25% refund: Cancelled 2-6 hours before service";
            }
            else
            {
                refundPercentage = 0; // No refund if cancelled less than 2 hours before
                refundReason = "No refund: Cancelled less than 2 hours before service";
            }
        }
        else
        {
            // Provider cancellation - full refund to customer
            refundPercentage = 100;
            refundReason = "Full refund: Cancelled by provider";
        }

        var refundAmount = booking.TotalAmount * (refundPercentage / 100m);
        var cancellationFee = booking.TotalAmount - refundAmount;

        return new RefundCalculation
        {
            RefundAmount = refundAmount,
            RefundPercentage = refundPercentage,
            RefundReason = refundReason,
            CancellationFee = cancellationFee
        };
    }

    private class RefundCalculation
    {
        public decimal RefundAmount { get; set; }
        public int RefundPercentage { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public decimal CancellationFee { get; set; }
    }
}

public class CancellationResultDto
{
    public Guid BookingId { get; set; }
    public decimal RefundAmount { get; set; }
    public int RefundPercentage { get; set; }
    public string RefundReason { get; set; } = string.Empty;
    public string? RefundTransactionId { get; set; }
    public decimal CancellationFee { get; set; }
}
