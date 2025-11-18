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

public class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, Result<BookingDto>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<AcceptBookingCommandHandler> _logger;

    public AcceptBookingCommandHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        ILogger<AcceptBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<BookingDto>.Failure("Booking not found");
            }

            // Verify provider owns this booking
            if (booking.ProviderId != request.ProviderId)
            {
                return Result<BookingDto>.Failure("You are not authorized to accept this booking");
            }

            // Check if booking is in correct status
            if (booking.Status != BookingStatus.Pending)
            {
                return Result<BookingDto>.Failure($"Cannot accept booking in {booking.Status} status");
            }

            // Update booking
            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = request.ProviderId.ToString();

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get related data for response and notifications
            var customer = await _userRepository.GetByIdAsync(booking.CustomerId, cancellationToken);
            var provider = booking.ProviderId.HasValue ? await _userRepository.GetByIdAsync(booking.ProviderId.Value, cancellationToken) : null;
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
            var address = await _addressRepository.GetByIdAsync(booking.AddressId, cancellationToken);

            // Send notifications
            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                try
                {
                    var bookingDetails = new BookingEmailDetails
                    {
                        BookingId = booking.Id.ToString(),
                        ServiceName = service?.NameEn ?? "Service",
                        ProviderName = provider?.FirstName + " " + provider?.LastName ?? "Provider",
                        ScheduledDate = booking.ScheduledAt,
                        Location = address?.Street ?? "Location",
                        TotalAmount = booking.TotalAmount,
                        Currency = booking.Currency.ToString()
                    };

                    await _emailService.SendBookingStatusUpdateEmailAsync(
                        customer.Email,
                        customer.FirstName + " " + customer.LastName,
                        booking.Id.ToString(),
                        "Confirmed - Provider Accepted"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification for booking acceptance");
                }
            }

            // Send push notification
            if (customer != null) // DeviceToken property doesn't exist in User
            {
                try
                {
                    // await _pushNotificationService.SendBookingStatusUpdateAsync(
                    //     customer.DeviceToken, // Property doesn't exist
                    //     booking.Id.ToString(),
                    //     "Confirmed"
                    // );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification for booking acceptance");
                }
            }

            // Map to DTO
            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                Status = booking.Status.ToString(),
                ServiceName = service?.NameEn ?? "Unknown Service",
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer",
                ProviderName = provider != null ? $"{provider.FirstName} {provider.LastName}" : "Unknown Provider",
                ScheduledAt = booking.ScheduledAt,
                AcceptedAt = DateTime.UtcNow,
                EstimatedArrival = request.EstimatedArrivalTime,
                TotalAmount = booking.TotalAmount,
                Currency = booking.Currency.ToString(),
                Address = address?.Street ?? "Unknown Address",
                SpecialInstructions = booking.SpecialInstructions,
                Notes = request.Notes
            };

            _logger.LogInformation("Booking {BookingId} accepted by provider {ProviderId}",
                request.BookingId, request.ProviderId);

            return Result<BookingDto>.Success(bookingDto, "Booking accepted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting booking {BookingId}", request.BookingId);
            return Result<BookingDto>.Failure("An error occurred while accepting the booking");
        }
    }
}
