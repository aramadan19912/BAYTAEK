using HomeService.Application.Common;
using HomeService.Application.DTOs.Payment;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Payments;

public record CreatePaymentIntentCommand(
    Guid BookingId,
    Guid CustomerId,
    PaymentMethod PaymentMethod = PaymentMethod.CreditCard
) : IRequest<Result<PaymentIntentDto>>;

public class CreatePaymentIntentCommandHandler
    : IRequestHandler<CreatePaymentIntentCommand, Result<PaymentIntentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentService;
    private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;

    public CreatePaymentIntentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGatewayService paymentService,
        ILogger<CreatePaymentIntentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<PaymentIntentDto>> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get booking with customer and service details
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<PaymentIntentDto>("Booking not found");

            // Validate customer ownership
            if (booking.CustomerId != request.CustomerId)
                return Result.Failure<PaymentIntentDto>("Unauthorized: Not your booking");

            // Check if booking already has payment
            var existingPayment = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .FirstOrDefaultAsync(p => p.BookingId == request.BookingId
                    && (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Processing),
                    cancellationToken);

            if (existingPayment != null)
                return Result.Failure<PaymentIntentDto>("Payment already exists for this booking");

            // Validate booking status
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                return Result.Failure<PaymentIntentDto>(
                    "Booking must be in Pending or Confirmed status to create payment");

            // Create payment DTO
            var createPaymentDto = new CreatePaymentDto
            {
                BookingId = booking.Id,
                PaymentMethod = request.PaymentMethod,
                Amount = booking.TotalAmount,
                Currency = booking.Currency.ToString(),
                CustomerEmail = booking.Customer.Email,
                CustomerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}",
                CustomerPhone = booking.Customer.PhoneNumber
            };

            // Create payment intent via gateway
            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                createPaymentDto,
                cancellationToken);

            // Create payment record in database
            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalAmount,
                Currency = booking.Currency,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending,
                PaymentGateway = "Stripe",
                TransactionId = null // Will be updated when payment completes
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Payment intent created for booking {BookingId}. Amount: {Amount} {Currency}",
                booking.Id, booking.TotalAmount, booking.Currency);

            return Result.Success(paymentIntent, "Payment intent created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for booking {BookingId}", request.BookingId);
            return Result.Failure<PaymentIntentDto>(
                "An error occurred while creating payment intent",
                ex.Message);
        }
    }
}
