using HomeService.Application.Commands.Booking;
using HomeService.Domain.Interfaces;
using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Booking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        IStripePaymentService stripePaymentService,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _stripePaymentService = stripePaymentService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<bool>.Failure("Booking not found");
            }

            // Authorization check
            if (booking.CustomerId != request.UserId && booking.ProviderId != request.UserId)
            {
                _logger.LogWarning("Unauthorized cancellation attempt for booking {BookingId} by user {UserId}",
                    request.BookingId, request.UserId);
                return Result<bool>.Failure("You are not authorized to cancel this booking");
            }

            // Check if booking can be cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                return Result<bool>.Failure("Booking is already cancelled");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                return Result<bool>.Failure("Cannot cancel a completed booking");
            }

            // Check cancellation policy (e.g., can't cancel if service started)
            if (booking.Status == BookingStatus.InProgress)
            {
                return Result<bool>.Failure("Cannot cancel booking while service is in progress");
            }

            // Calculate refund eligibility
            var hoursUntilBooking = (booking.ScheduledAt - DateTime.UtcNow).TotalHours;
            decimal refundPercentage = CalculateRefundPercentage(hoursUntilBooking, booking.Status);

            // Update booking status
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = request.UserId.ToString();

            // Store cancellation details
            var cancellationNote = $"Cancelled by {(request.IsCustomerCancellation ? "customer" : "provider")}. " +
                                 $"Reason: {request.Reason}. Refund: {refundPercentage}%";
            booking.SpecialInstructions = string.IsNullOrEmpty(booking.SpecialInstructions)
                ? cancellationNote
                : $"{booking.SpecialInstructions}\n\n{cancellationNote}";

            await _bookingRepository.UpdateAsync(booking, cancellationToken);

            // Process refund if payment was made
            var payment = await _paymentRepository.FindAsync(
                p => p.BookingId == booking.Id && p.Status == PaymentStatus.Completed,
                cancellationToken);

            if (payment != null && payment.Any() && refundPercentage > 0)
            {
                try
                {
                    var completedPayment = payment.First();
                    var refundAmount = completedPayment.Amount * (refundPercentage / 100);

                    var refundResult = await _stripePaymentService.ProcessRefundAsync(
                        completedPayment.TransactionId,
                        refundAmount,
                        $"Booking cancelled: {request.Reason}"
                    );

                    if (refundResult.IsSuccess)
                    {
                        completedPayment.RefundAmount = refundAmount;
                        completedPayment.RefundedAt = DateTime.UtcNow;
                        completedPayment.RefundReason = request.Reason;
                        completedPayment.Status = refundPercentage == 100
                            ? PaymentStatus.Refunded
                            : PaymentStatus.PartiallyRefunded;

                        await _paymentRepository.UpdateAsync(completedPayment, cancellationToken);

                        _logger.LogInformation("Refund processed for booking {BookingId}. Amount: {Amount} {Currency}",
                            booking.Id, refundAmount, completedPayment.Currency);
                    }
                    else
                    {
                        _logger.LogError("Refund failed for booking {BookingId}: {Error}",
                            booking.Id, refundResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund for booking {BookingId}", booking.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notifications
            var customer = await _userRepository.GetByIdAsync(booking.CustomerId, cancellationToken);
            var provider = await _userRepository.GetByIdAsync(booking.ProviderId, cancellationToken);
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);

            // Notify the other party (customer or provider)
            var recipientUser = request.IsCustomerCancellation ? provider : customer;
            var recipientRole = request.IsCustomerCancellation ? "Provider" : "Customer";

            if (recipientUser != null)
            {
                // Email notification
                if (!string.IsNullOrEmpty(recipientUser.Email))
                {
                    try
                    {
                        await _emailService.SendBookingStatusUpdateEmailAsync(
                            recipientUser.Email,
                            $"{recipientUser.FirstName} {recipientUser.LastName}",
                            booking.Id.ToString(),
                            "Cancelled"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification for booking cancellation");
                    }
                }

                // Push notification
                if (!string.IsNullOrEmpty(recipientUser.DeviceToken))
                {
                    try
                    {
                        await _pushNotificationService.SendBookingStatusUpdateAsync(
                            recipientUser.DeviceToken,
                            booking.Id.ToString(),
                            "Cancelled"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send push notification for booking cancellation");
                    }
                }
            }

            _logger.LogInformation("Booking {BookingId} cancelled by {CancelledBy}. Refund: {RefundPercentage}%",
                booking.Id, request.IsCustomerCancellation ? "customer" : "provider", refundPercentage);

            var message = refundPercentage > 0
                ? $"Booking cancelled successfully. A {refundPercentage}% refund will be processed."
                : "Booking cancelled successfully.";

            return Result<bool>.Success(true, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
            return Result<bool>.Failure("An error occurred while cancelling the booking");
        }
    }

    private decimal CalculateRefundPercentage(double hoursUntilBooking, BookingStatus status)
    {
        // Cancellation policy:
        // - If booking is pending (not confirmed): 100% refund
        // - More than 24 hours before: 100% refund
        // - 12-24 hours before: 50% refund
        // - Less than 12 hours before: 0% refund (no refund)
        // - If service confirmed and provider is on the way or arrived: 0% refund

        if (status == BookingStatus.Pending)
        {
            return 100m;
        }

        if (hoursUntilBooking > 24)
        {
            return 100m;
        }
        else if (hoursUntilBooking >= 12)
        {
            return 50m;
        }
        else
        {
            return 0m;
        }
    }
}
