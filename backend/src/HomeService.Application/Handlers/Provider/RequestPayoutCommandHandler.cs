using HomeService.Application.Commands.Provider;
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

namespace HomeService.Application.Handlers.Provider;

public class RequestPayoutCommandHandler : IRequestHandler<RequestPayoutCommand, Result<PayoutRequestDto>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<Domain.Entities.Payment> _paymentRepository;
    // TODO: Add IRepository<Payout> _payoutRepository when Payout entity is created
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<RequestPayoutCommandHandler> _logger;

    // Payout configuration
    private const decimal PlatformFeePercentage = 15m; // 15% platform fee
    private const decimal InstantPayoutFeePercentage = 2m; // Additional 2% for instant payout
    private const decimal MinimumPayoutAmount = 50m; // Minimum 50 SAR
    private const int StandardPayoutDays = 3; // Standard payout takes 3 business days

    public RequestPayoutCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<Domain.Entities.Payment> paymentRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<RequestPayoutCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<PayoutRequestDto>> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<PayoutRequestDto>.Failure("Provider not found");
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                return Result<PayoutRequestDto>.Failure("Payout amount must be greater than zero");
            }

            if (request.Amount < MinimumPayoutAmount)
            {
                return Result<PayoutRequestDto>.Failure($"Minimum payout amount is {MinimumPayoutAmount} SAR");
            }

            // Calculate available balance
            var completedBookings = await _bookingRepository.FindAsync(
                b => b.ProviderId == request.ProviderId && b.Status == BookingStatus.Completed,
                cancellationToken);

            var completedBookingIds = completedBookings?.Select(b => b.Id).ToList() ?? new List<Guid>();

            var completedPayments = await _paymentRepository.FindAsync(
                p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed,
                cancellationToken);

            var totalRevenue = completedPayments?.Sum(p => p.Amount) ?? 0;

            // Calculate available balance after platform fee
            var platformFeeAmount = totalRevenue * (PlatformFeePercentage / 100);
            var availableBalance = totalRevenue - platformFeeAmount;

            // TODO: Subtract already paid out amounts when Payout entity is implemented
            // For now, we'll assume availableBalance is correct

            // Validate sufficient balance
            if (request.Amount > availableBalance)
            {
                return Result<PayoutRequestDto>.Failure(
                    $"Insufficient balance. Available: {availableBalance:F2} SAR, Requested: {request.Amount:F2} SAR");
            }

            // Validate bank account details
            if (string.IsNullOrWhiteSpace(provider.BankAccountNumber) && string.IsNullOrWhiteSpace(provider.IbanNumber))
            {
                return Result<PayoutRequestDto>.Failure(
                    "Please add your bank account details before requesting a payout");
            }

            // Calculate fees and net amount
            decimal payoutFee = 0;
            if (request.IsInstant)
            {
                payoutFee = request.Amount * (InstantPayoutFeePercentage / 100);
            }

            var netAmount = request.Amount - payoutFee;

            // Calculate estimated arrival date
            DateTime? estimatedArrival = null;
            if (request.IsInstant)
            {
                estimatedArrival = DateTime.UtcNow.AddHours(1); // Instant: within 1 hour
            }
            else
            {
                // Standard: 3 business days (skip weekends)
                var daysToAdd = 0;
                var currentDate = DateTime.UtcNow;
                while (daysToAdd < StandardPayoutDays)
                {
                    currentDate = currentDate.AddDays(1);
                    if (currentDate.DayOfWeek != DayOfWeek.Friday && currentDate.DayOfWeek != DayOfWeek.Saturday)
                    {
                        daysToAdd++;
                    }
                }
                estimatedArrival = currentDate;
            }

            // TODO: Create Payout entity and save to database
            // For now, we'll create a mock payout ID
            var payoutId = Guid.NewGuid();

            /*
            var payout = new Payout
            {
                Id = payoutId,
                ProviderId = request.ProviderId,
                Amount = request.Amount,
                Fee = payoutFee,
                NetAmount = netAmount,
                PayoutMethod = request.IsInstant ? PayoutMethod.Instant : PayoutMethod.Standard,
                Status = PayoutStatus.Pending,
                BankAccountNumber = provider.BankAccountNumber,
                BankName = provider.BankName,
                IbanNumber = provider.IbanNumber,
                RequestedAt = DateTime.UtcNow,
                EstimatedArrivalDate = estimatedArrival,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ProviderId.ToString()
            };

            await _payoutRepository.AddAsync(payout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            */

            _logger.LogInformation("Payout requested by provider {ProviderId}. Amount: {Amount}, Method: {Method}",
                request.ProviderId, request.Amount, request.IsInstant ? "Instant" : "Standard");

            // TODO: Send notification email
            /*
            try
            {
                await _emailService.SendPayoutRequestConfirmationEmailAsync(
                    provider.Email,
                    provider.FirstName,
                    payoutId.ToString(),
                    netAmount,
                    estimatedArrival,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payout confirmation email to provider {ProviderId}", request.ProviderId);
            }
            */

            var responseDto = new PayoutRequestDto
            {
                PayoutId = payoutId,
                Amount = request.Amount,
                Fee = payoutFee,
                NetAmount = netAmount,
                PayoutMethod = request.IsInstant ? "instant" : "standard",
                Status = "pending",
                RequestedAt = DateTime.UtcNow,
                EstimatedArrivalDate = estimatedArrival,
                Message = request.IsInstant
                    ? $"Your instant payout of {netAmount:F2} SAR is being processed and will arrive within 1 hour."
                    : $"Your payout of {netAmount:F2} SAR will arrive by {estimatedArrival:MMM dd, yyyy}."
            };

            return Result<PayoutRequestDto>.Success(responseDto, "Payout request submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payout request for provider {ProviderId}", request.ProviderId);
            return Result<PayoutRequestDto>.Failure("An error occurred while processing the payout request");
        }
    }
}
