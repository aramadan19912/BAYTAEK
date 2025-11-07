using HomeService.Application.DTOs.Payment;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IStripePaymentService stripePaymentService,
        IRepository<Payment> paymentRepository,
        IRepository<Booking> bookingRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentsController> logger)
    {
        _stripePaymentService = stripePaymentService;
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Create a payment intent for booking payment
    /// </summary>
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify booking exists and belongs to user
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
            if (booking.CustomerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Create payment intent
            var paymentIntent = await _stripePaymentService.CreatePaymentIntentAsync(request, cancellationToken);

            _logger.LogInformation("Payment intent created for booking {BookingId} by user {UserId}",
                request.BookingId, userId);

            return Ok(new
            {
                success = true,
                data = paymentIntent
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for booking {BookingId}", request.BookingId);
            return BadRequest(new { message = "Failed to create payment intent", error = ex.Message });
        }
    }

    /// <summary>
    /// Process payment for a booking
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDto request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
            if (booking.CustomerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Check if payment already exists
            var existingPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var existingPayment = existingPayments.FirstOrDefault(p => p.BookingId == request.BookingId &&
                                                                        p.Status == PaymentStatus.Completed);
            if (existingPayment != null)
            {
                return BadRequest(new { message = "Payment already completed for this booking" });
            }

            // Process payment through gateway
            var paymentResponse = await _stripePaymentService.ProcessPaymentAsync(request, cancellationToken);

            // Create payment record
            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = paymentResponse.Status,
                TransactionId = paymentResponse.TransactionId,
                GatewayResponse = paymentResponse.GatewayResponse,
                ProcessedAt = paymentResponse.Status == PaymentStatus.Completed ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Update booking status if payment successful
            if (paymentResponse.Status == PaymentStatus.Completed)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = userId.ToString();
                await _bookingRepository.UpdateAsync(booking, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment processed for booking {BookingId}: Status={Status}, TransactionId={TransactionId}",
                request.BookingId, paymentResponse.Status, paymentResponse.TransactionId);

            return Ok(new
            {
                success = paymentResponse.Status == PaymentStatus.Completed,
                data = new
                {
                    paymentId = payment.Id,
                    transactionId = paymentResponse.TransactionId,
                    status = paymentResponse.Status.ToString(),
                    amount = paymentResponse.Amount,
                    currency = paymentResponse.Currency,
                    requiresAction = paymentResponse.RequiresAction,
                    clientSecret = paymentResponse.ClientSecret,
                    errorMessage = paymentResponse.ErrorMessage
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", request.BookingId);
            return BadRequest(new { message = "Payment processing failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Verify payment status
    /// </summary>
    [HttpGet("{paymentId}/verify")]
    public async Task<IActionResult> VerifyPayment(Guid paymentId, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            // Verify with gateway
            if (!string.IsNullOrEmpty(payment.TransactionId))
            {
                var gatewayStatus = await _stripePaymentService.GetPaymentStatusAsync(payment.TransactionId, cancellationToken);

                // Update local status if different
                var newStatus = MapGatewayStatusToPaymentStatus(gatewayStatus);
                if (payment.Status != newStatus)
                {
                    payment.Status = newStatus;
                    if (newStatus == PaymentStatus.Completed)
                    {
                        payment.ProcessedAt = DateTime.UtcNow;
                    }
                    await _paymentRepository.UpdateAsync(payment, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    paymentId = payment.Id,
                    transactionId = payment.TransactionId,
                    status = payment.Status.ToString(),
                    amount = payment.Amount,
                    processedAt = payment.ProcessedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment {PaymentId}", paymentId);
            return BadRequest(new { message = "Payment verification failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Process refund (Admin only)
    /// </summary>
    [HttpPost("{paymentId}/refund")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ProcessRefund(Guid paymentId, [FromBody] ProcessRefundDto request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                return BadRequest(new { message = "Can only refund completed payments" });
            }

            if (string.IsNullOrEmpty(payment.TransactionId))
            {
                return BadRequest(new { message = "No transaction ID found for this payment" });
            }

            // Process refund through gateway
            var refundResponse = await _stripePaymentService.ProcessRefundAsync(
                payment.TransactionId,
                request.Amount,
                request.Reason,
                cancellationToken);

            if (!string.IsNullOrEmpty(refundResponse.ErrorMessage))
            {
                return BadRequest(new { message = "Refund failed", error = refundResponse.ErrorMessage });
            }

            // Update payment status
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = request.AdminUserId.ToString();
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Update booking status
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, cancellationToken);
            if (booking != null)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = request.AdminUserId.ToString();
                await _bookingRepository.UpdateAsync(booking, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refund processed for payment {PaymentId}: Amount={Amount}, RefundId={RefundId}",
                paymentId, request.Amount, refundResponse.RefundTransactionId);

            return Ok(new
            {
                success = true,
                data = new
                {
                    refundId = refundResponse.RefundTransactionId,
                    amount = refundResponse.Amount,
                    status = refundResponse.Status,
                    processedAt = refundResponse.ProcessedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
            return BadRequest(new { message = "Refund processing failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Get payment history for user
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);

            var userBookingIds = allBookings
                .Where(b => b.CustomerId == userId)
                .Select(b => b.Id)
                .ToList();

            var payments = allPayments
                .Where(p => userBookingIds.Contains(p.BookingId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.Id,
                    bookingId = p.BookingId,
                    amount = p.Amount,
                    paymentMethod = p.PaymentMethod.ToString(),
                    status = p.Status.ToString(),
                    transactionId = p.TransactionId,
                    createdAt = p.CreatedAt,
                    processedAt = p.ProcessedAt
                })
                .ToList();

            var totalCount = allPayments.Count(p => userBookingIds.Contains(p.BookingId));

            return Ok(new
            {
                success = true,
                data = payments,
                pagination = new
                {
                    pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history");
            return BadRequest(new { message = "Failed to retrieve payment history", error = ex.Message });
        }
    }

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
    [HttpPost("webhook/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
            var signature = Request.Headers["Stripe-Signature"].ToString();

            var success = await _stripePaymentService.HandleWebhookAsync(json, signature, cancellationToken);

            return success ? Ok() : BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return BadRequest();
        }
    }

    private PaymentStatus MapGatewayStatusToPaymentStatus(string gatewayStatus)
    {
        return gatewayStatus.ToLower() switch
        {
            "succeeded" => PaymentStatus.Completed,
            "processing" => PaymentStatus.Processing,
            "pending" => PaymentStatus.Pending,
            "failed" => PaymentStatus.Failed,
            "canceled" => PaymentStatus.Failed,
            _ => PaymentStatus.Pending
        };
    }
}
