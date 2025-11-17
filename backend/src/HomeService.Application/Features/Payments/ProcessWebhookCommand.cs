// DISABLED: Requires Stripe package which violates clean architecture
// using HomeService.Application.Common;
// using HomeService.Application.Interfaces;
// using HomeService.Domain.Entities;
// using HomeService.Domain.Enums;
// using HomeService.Domain.Interfaces;
// using MediatR;
// using Microsoft.Extensions.Logging;
// 
// namespace HomeService.Application.Features.Payments;
// 
// public record ProcessWebhookCommand(string Payload, string Signature) : IRequest<Result<bool>>;
// 
// public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, Result<bool>>
// {
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly INotificationService _notificationService;
//     private readonly ILogger<ProcessWebhookCommandHandler> _logger;
// 
//     public ProcessWebhookCommandHandler(
//         IUnitOfWork unitOfWork,
//         INotificationService notificationService,
//         ILogger<ProcessWebhookCommandHandler> logger)
//     {
//         _unitOfWork = unitOfWork;
//         _notificationService = notificationService;
//         _logger = logger;
//     }
// 
//     public async Task<Result<bool>> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
//     {
//         try
//         {
//             // Parse the Stripe event (signature verification happens in controller)
//             var stripeEvent = EventUtility.ParseEvent(request.Payload);
// 
//             _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);
// 
//             switch (stripeEvent.Type)
//             {
//                 case Events.PaymentIntentSucceeded:
//                     await HandlePaymentSucceeded(stripeEvent, cancellationToken);
//                     break;
// 
//                 case Events.PaymentIntentPaymentFailed:
//                     await HandlePaymentFailed(stripeEvent, cancellationToken);
//                     break;
// 
//                 case Events.PaymentIntentCanceled:
//                     await HandlePaymentCanceled(stripeEvent, cancellationToken);
//                     break;
// 
//                 case Events.ChargeRefunded:
//                     await HandleRefund(stripeEvent, cancellationToken);
//                     break;
// 
//                 case Events.PaymentIntentProcessing:
//                     await HandlePaymentProcessing(stripeEvent, cancellationToken);
//                     break;
// 
//                 default:
//                     _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
//                     break;
//             }
// 
//             return Result.Success(true, "Webhook processed successfully");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error processing webhook");
//             return Result.Failure<bool>("Error processing webhook", ex.Message);
//         }
//     }
// 
//     private async Task HandlePaymentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
//     {
//         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//         if (paymentIntent == null) return;
// 
//         var bookingId = Guid.Parse(paymentIntent.Metadata["booking_id"]);
// 
//         // Update payment record
//         var payment = await _unitOfWork.Repository<Payment>()
//             .GetQueryable()
//             .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
// 
//         if (payment == null)
//         {
//             _logger.LogWarning("Payment not found for booking {BookingId}", bookingId);
//             return;
//         }
// 
//         payment.Status = PaymentStatus.Completed;
//         payment.TransactionId = paymentIntent.Id;
//         payment.ProcessedAt = DateTime.UtcNow;
//         payment.GatewayResponse = paymentIntent.Status;
//         _unitOfWork.Repository<Payment>().Update(payment);
// 
//         // Update booking status to Confirmed
//         var booking = await _unitOfWork.Repository<Booking>()
//             .GetQueryable()
//             .Include(b => b.Customer)
//             .Include(b => b.Service)
//             .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
// 
//         if (booking != null && booking.Status == BookingStatus.Pending)
//         {
//             booking.Status = BookingStatus.Confirmed;
//             _unitOfWork.Repository<Booking>().Update(booking);
// 
//             // Create booking history
//             var history = new BookingHistory
//             {
//                 BookingId = booking.Id,
//                 Status = BookingStatus.Confirmed,
//                 ChangedBy = booking.CustomerId,
//                 Notes = $"Payment completed. Transaction ID: {paymentIntent.Id}"
//             };
//             await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);
// 
//             // Send payment confirmation notification
//             _ = Task.Run(async () =>
//             {
//                 try
//                 {
//                     await _notificationService.SendPaymentConfirmationAsync(
//                         booking.CustomerId,
//                         booking.Id,
//                         payment.Amount,
//                         payment.Currency.ToString(),
//                         CancellationToken.None);
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Error sending payment confirmation notification");
//                 }
//             }, cancellationToken);
//         }
// 
//         await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//         _logger.LogInformation(
//             "Payment succeeded for booking {BookingId}. Transaction: {TransactionId}",
//             bookingId, paymentIntent.Id);
//     }
// 
//     private async Task HandlePaymentFailed(Event stripeEvent, CancellationToken cancellationToken)
//     {
//         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//         if (paymentIntent == null) return;
// 
//         var bookingId = Guid.Parse(paymentIntent.Metadata["booking_id"]);
// 
//         var payment = await _unitOfWork.Repository<Payment>()
//             .GetQueryable()
//             .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
// 
//         if (payment == null) return;
// 
//         payment.Status = PaymentStatus.Failed;
//         payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
//         payment.GatewayResponse = paymentIntent.Status;
//         _unitOfWork.Repository<Payment>().Update(payment);
// 
//         await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//         _logger.LogWarning(
//             "Payment failed for booking {BookingId}. Reason: {Reason}",
//             bookingId, payment.FailureReason);
//     }
// 
//     private async Task HandlePaymentCanceled(Event stripeEvent, CancellationToken cancellationToken)
//     {
//         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//         if (paymentIntent == null) return;
// 
//         var bookingId = Guid.Parse(paymentIntent.Metadata["booking_id"]);
// 
//         var payment = await _unitOfWork.Repository<Payment>()
//             .GetQueryable()
//             .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
// 
//         if (payment == null) return;
// 
//         payment.Status = PaymentStatus.Failed;
//         payment.FailureReason = "Payment canceled by user";
//         payment.GatewayResponse = paymentIntent.Status;
//         _unitOfWork.Repository<Payment>().Update(payment);
// 
//         await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//         _logger.LogInformation("Payment canceled for booking {BookingId}", bookingId);
//     }
// 
//     private async Task HandlePaymentProcessing(Event stripeEvent, CancellationToken cancellationToken)
//     {
//         var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
//         if (paymentIntent == null) return;
// 
//         var bookingId = Guid.Parse(paymentIntent.Metadata["booking_id"]);
// 
//         var payment = await _unitOfWork.Repository<Payment>()
//             .GetQueryable()
//             .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
// 
//         if (payment == null) return;
// 
//         payment.Status = PaymentStatus.Processing;
//         payment.GatewayResponse = paymentIntent.Status;
//         _unitOfWork.Repository<Payment>().Update(payment);
// 
//         await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//         _logger.LogInformation("Payment processing for booking {BookingId}", bookingId);
//     }
// 
//     private async Task HandleRefund(Event stripeEvent, CancellationToken cancellationToken)
//     {
//         var charge = stripeEvent.Data.Object as Charge;
//         if (charge == null) return;
// 
//         // Find payment by transaction ID
//         var payment = await _unitOfWork.Repository<Payment>()
//             .GetQueryable()
//             .Include(p => p.Booking)
//                 .ThenInclude(b => b.Customer)
//             .FirstOrDefaultAsync(p => p.TransactionId == charge.PaymentIntentId, cancellationToken);
// 
//         if (payment == null) return;
// 
//         var refundAmount = charge.AmountRefunded / 100m; // Convert from cents
// 
//         payment.Status = refundAmount >= payment.Amount
//             ? PaymentStatus.Refunded
//             : PaymentStatus.PartiallyRefunded;
//         payment.RefundAmount = refundAmount;
//         payment.RefundedAt = DateTime.UtcNow;
//         _unitOfWork.Repository<Payment>().Update(payment);
// 
//         await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//         _logger.LogInformation(
//             "Refund processed for booking {BookingId}. Amount: {Amount}",
//             payment.BookingId, refundAmount);
//     }
// }
// 
