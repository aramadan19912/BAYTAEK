using AutoMapper;
using HomeService.Application.Commands.Payments;
using HomeService.Application.Common;
using HomeService.Application.Mappings;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Payments;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result.Failure<PaymentDto>("Booking not found");
            }

            // Check if payment already exists
            var existingPayments = await _paymentRepository.FindAsync(
                p => p.BookingId == request.BookingId && p.Status == PaymentStatus.Completed,
                cancellationToken);

            if (existingPayments.Any())
            {
                return Result.Failure<PaymentDto>("Payment already processed for this booking");
            }

            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = request.BookingId,
                Amount = booking.TotalAmount,
                Currency = booking.Currency,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);

            // Simulate payment gateway integration
            // In production, integrate with actual payment gateways (Stripe, HyperPay, etc.)
            var paymentSuccess = await SimulatePaymentGateway(request.PaymentMethod, booking.TotalAmount, request.PaymentToken);

            if (paymentSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString("N");

                // Update booking status
                if (booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Confirmed;
                }

                await _bookingRepository.UpdateAsync(booking, cancellationToken);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Payment gateway declined the transaction";
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            _logger.LogInformation("Payment {Status} for booking {BookingId}", payment.Status, request.BookingId);

            return Result.Success(paymentDto, $"Payment {payment.Status.ToString().ToLower()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return Result.Failure<PaymentDto>("An error occurred while processing payment", ex.Message);
        }
    }

    private async Task<bool> SimulatePaymentGateway(PaymentMethod method, decimal amount, string? token)
    {
        // Simulate payment gateway delay
        await Task.Delay(1000);

        // Simulate 95% success rate
        return new Random().Next(100) < 95;
    }
}
