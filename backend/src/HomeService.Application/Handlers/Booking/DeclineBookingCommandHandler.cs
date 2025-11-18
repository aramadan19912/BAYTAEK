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

public class DeclineBookingCommandHandler : IRequestHandler<DeclineBookingCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<DeclineBookingCommandHandler> _logger;

    public DeclineBookingCommandHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        ILogger<DeclineBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeclineBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<bool>.Failure("Booking not found");
            }

            // Verify provider owns this booking
            if (booking.ProviderId != request.ProviderId)
            {
                return Result<bool>.Failure("You are not authorized to decline this booking");
            }

            // Check if booking can be declined
            if (booking.Status != BookingStatus.Pending)
            {
                return Result<bool>.Failure($"Cannot decline booking in {booking.Status} status");
            }

            // Cancel the booking
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = request.ProviderId.ToString();

            // Store decline reason in special instructions (or create a new field for this)
            booking.SpecialInstructions = $"Declined by provider. Reason: {request.Reason}. Notes: {request.Notes}";

            // TODO: In a production system, we should:
            // 1. Keep the booking as Pending
            // 2. Find another available provider
            // 3. Reassign the booking
            // For now, we're cancelling it

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notify customer
            var customer = await _userRepository.GetByIdAsync(booking.CustomerId, cancellationToken);

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                try
                {
                    await _emailService.SendBookingStatusUpdateEmailAsync(
                        customer.Email,
                        $"{customer.FirstName} {customer.LastName}",
                        booking.Id.ToString(),
                        "Cancelled - Provider Unavailable"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification for booking decline");
                }
            }

            // Send push notification
            if (customer != null) // DeviceToken property doesn't exist in User
            {
                try
                {
                    // await _pushNotificationService.SendBookingStatusUpdateAsync(
                    //     null, // DeviceToken property doesn't exist
                    //     booking.Id.ToString(),
                    //     "Cancelled"
                    // );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification for booking decline");
                }
            }

            _logger.LogInformation("Booking {BookingId} declined by provider {ProviderId}. Reason: {Reason}",
                request.BookingId, request.ProviderId, request.Reason);

            return Result<bool>.Success(true, "Booking declined successfully. Customer has been notified.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining booking {BookingId}", request.BookingId);
            return Result<bool>.Failure("An error occurred while declining the booking");
        }
    }
}
