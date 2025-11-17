using HomeService.Application.Common;
using HomeService.Application.DTOs.Payment;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Payments;

public record ProcessRefundCommand(
    Guid PaymentId,
    decimal? RefundAmount = null, // If null, refund full amount
    string Reason = "Booking cancelled"
) : IRequest<Result<RefundResponseDto>>;

public class ProcessRefundCommandHandler
    : IRequestHandler<ProcessRefundCommand, Result<RefundResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProcessRefundCommandHandler> _logger;

    public ProcessRefundCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGatewayService paymentService,
        INotificationService notificationService,
        ILogger<ProcessRefundCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<RefundResponseDto>> Handle(
        ProcessRefundCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get payment with booking and customer details
            var payment = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(p => p.Booking.Service)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
                return Result.Failure<RefundResponseDto>("Payment not found");

            // Validate payment status
            if (payment.Status != PaymentStatus.Completed)
                return Result.Failure<RefundResponseDto>(
                    $"Cannot refund payment with status {payment.Status}. Only completed payments can be refunded.");

            // Check if already refunded
            if (payment.Status == PaymentStatus.Refunded)
                return Result.Failure<RefundResponseDto>("Payment has already been fully refunded");

            // Calculate refund amount
            var refundAmount = request.RefundAmount ?? payment.Amount;
            var previousRefundAmount = payment.RefundAmount ?? 0;
            var totalRefundAmount = previousRefundAmount + refundAmount;

            // Validate refund amount
            if (totalRefundAmount > payment.Amount)
                return Result.Failure<RefundResponseDto>(
                    $"Refund amount ({totalRefundAmount}) cannot exceed payment amount ({payment.Amount})");

            if (string.IsNullOrEmpty(payment.TransactionId))
                return Result.Failure<RefundResponseDto>("Payment transaction ID not found");

            // Process refund via payment gateway
            var refundResponse = await _paymentService.ProcessRefundAsync(
                payment.TransactionId,
                refundAmount,
                request.Reason,
                cancellationToken);

            if (string.IsNullOrEmpty(refundResponse.Status) || refundResponse.Status.ToLower() != "succeeded")
            {
                _logger.LogError(
                    "Refund failed for payment {PaymentId}. Error: {Error}",
                    request.PaymentId, refundResponse.ErrorMessage);

                return Result.Failure<RefundResponseDto>(
                    $"Refund processing failed: {refundResponse.ErrorMessage}");
            }

            // Update payment record
            payment.RefundAmount = totalRefundAmount;
            payment.RefundedAt = DateTime.UtcNow;
            payment.RefundReason = request.Reason;
            payment.Status = totalRefundAmount >= payment.Amount
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;

            _unitOfWork.Repository<Payment>().Update(payment);

            // Create booking history
            var history = new BookingHistory
            {
                BookingId = payment.BookingId,
                Status = payment.Booking.Status,
                ChangedBy = payment.Booking.CustomerId, // System change
                Notes = $"Refund processed: {refundAmount} {payment.Currency}. " +
                       $"Reason: {request.Reason}. " +
                       $"Transaction ID: {refundResponse.RefundTransactionId}"
            };
            await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send refund notification
            _ = Task.Run(async () =>
            {
                try
                {
                    var titleEn = "Refund Processed";
                    var titleAr = "تمت معالجة المبلغ المسترد";
                    var messageEn = $"A refund of {refundAmount} {payment.Currency} has been processed for your booking. " +
                                   $"It will appear in your account within 5-10 business days.";
                    var messageAr = $"تمت معالجة مبلغ مسترد قدره {refundAmount} {payment.Currency} لحجزك. " +
                                   $"سيظهر في حسابك خلال 5-10 أيام عمل.";

                    await _notificationService.SendNotificationAsync(
                        payment.Booking.CustomerId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Payment,
                        payment.BookingId,
                        nameof(Booking),
                        $"/bookings/{payment.BookingId}",
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending refund notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Refund processed successfully for payment {PaymentId}. Amount: {Amount}, Transaction: {TransactionId}",
                request.PaymentId, refundAmount, refundResponse.RefundTransactionId);

            return Result.Success(refundResponse, "Refund processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
            return Result.Failure<RefundResponseDto>(
                "An error occurred while processing the refund",
                ex.Message);
        }
    }
}
