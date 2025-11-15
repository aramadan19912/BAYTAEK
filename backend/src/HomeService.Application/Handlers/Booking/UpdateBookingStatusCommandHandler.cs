using HomeService.Application.Commands.Booking;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Booking;

public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, Result<BookingDto>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<UpdateBookingStatusCommandHandler> _logger;

    public UpdateBookingStatusCommandHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<Domain.Entities.User> userRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        IRepository<Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPushNotificationService pushNotificationService,
        ILogger<UpdateBookingStatusCommandHandler> logger)
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

    public async Task<Result<BookingDto>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("Provider {ProviderId} attempted to update booking {BookingId} they don't own",
                    request.ProviderId, request.BookingId);
                return Result<BookingDto>.Failure("You are not authorized to update this booking");
            }

            // Parse and validate status
            var newStatus = ParseStatus(request.Status);
            if (!newStatus.HasValue)
            {
                return Result<BookingDto>.Failure($"Invalid status: {request.Status}. Valid statuses are: on_the_way, arrived, in_progress, completed");
            }

            // Validate status transition
            var validationResult = ValidateStatusTransition(booking.Status, newStatus.Value);
            if (!validationResult.IsValid)
            {
                return Result<BookingDto>.Failure(validationResult.ErrorMessage);
            }

            // Get current status before update for notifications
            var oldStatus = booking.Status;

            // Update booking status
            booking.Status = newStatus.Value;

            // Update timestamps based on status
            switch (newStatus.Value)
            {
                case BookingStatus.InProgress:
                    if (booking.StartedAt == null)
                    {
                        booking.StartedAt = DateTime.UtcNow;
                    }
                    break;

                case BookingStatus.Completed:
                    if (booking.CompletedAt == null)
                    {
                        booking.CompletedAt = DateTime.UtcNow;
                    }
                    if (booking.StartedAt == null)
                    {
                        booking.StartedAt = DateTime.UtcNow;
                    }
                    break;
            }

            // Add notes if provided
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                booking.ProviderNotes = request.Notes;
            }

            // Store photo URLs if provided (for completed status)
            if (request.PhotoUrls != null && request.PhotoUrls.Any() && newStatus.Value == BookingStatus.Completed)
            {
                // TODO: Store completion photos in a separate table or as JSON in booking
                // For now, we'll append to provider notes
                booking.ProviderNotes = (booking.ProviderNotes ?? "") +
                    $"\nCompletion Photos: {string.Join(", ", request.PhotoUrls)}";
            }

            // Update audit fields
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = request.ProviderId.ToString();

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Booking {BookingId} status updated from {OldStatus} to {NewStatus} by provider {ProviderId}",
                request.BookingId, oldStatus, newStatus.Value, request.ProviderId);

            // Get related entities for notifications and DTO
            var customer = await _userRepository.GetByIdAsync(booking.CustomerId, cancellationToken);
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
            var address = booking.AddressId.HasValue
                ? await _addressRepository.GetByIdAsync(booking.AddressId.Value, cancellationToken)
                : null;

            // Send notification to customer
            if (customer != null && service != null)
            {
                var statusDisplayName = GetStatusDisplayName(newStatus.Value);

                // Send email notification
                try
                {
                    await _emailService.SendBookingStatusUpdateEmailAsync(
                        customer.Email,
                        customer.FirstName,
                        booking.Id.ToString(),
                        service.NameEn,
                        statusDisplayName,
                        customer.PreferredLanguage,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send status update email for booking {BookingId}", booking.Id);
                }

                // Send push notification
                try
                {
                    var pushMessage = customer.PreferredLanguage == "ar"
                        ? $"تم تحديث حالة حجزك: {statusDisplayName}"
                        : $"Your booking status has been updated: {statusDisplayName}";

                    await _pushNotificationService.SendBookingStatusUpdateAsync(
                        booking.CustomerId,
                        booking.Id,
                        statusDisplayName,
                        pushMessage,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push notification for booking {BookingId}", booking.Id);
                }
            }

            // Build response DTO
            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                Status = booking.Status.ToString(),
                ScheduledAt = booking.ScheduledAt,
                StartedAt = booking.StartedAt,
                CompletedAt = booking.CompletedAt,
                TotalAmount = booking.TotalAmount,
                Currency = booking.Currency,
                ServiceName = service?.NameEn ?? "",
                ServiceNameAr = service?.NameAr ?? "",
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "",
                CustomerPhone = customer?.PhoneNumber ?? "",
                Address = address?.FullAddress ?? "",
                Latitude = address?.Latitude ?? 0,
                Longitude = address?.Longitude ?? 0
            };

            return Result<BookingDto>.Success(bookingDto, $"Booking status updated to {GetStatusDisplayName(newStatus.Value)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for booking {BookingId}", request.BookingId);
            return Result<BookingDto>.Failure("An error occurred while updating the booking status");
        }
    }

    private BookingStatus? ParseStatus(string status)
    {
        return status?.ToLower() switch
        {
            "on_the_way" => BookingStatus.Confirmed, // Provider is on the way (still confirmed)
            "arrived" => BookingStatus.Confirmed, // Provider has arrived (still confirmed)
            "in_progress" => BookingStatus.InProgress,
            "completed" => BookingStatus.Completed,
            _ => null
        };
    }

    private (bool IsValid, string ErrorMessage) ValidateStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        // Valid status transitions:
        // Confirmed -> InProgress
        // InProgress -> Completed
        // (on_the_way and arrived are mapped to Confirmed, so they're always valid from Confirmed)

        if (currentStatus == newStatus)
        {
            return (false, "Booking is already in this status");
        }

        switch (currentStatus)
        {
            case BookingStatus.Pending:
                return (false, "Please accept the booking first before updating its status");

            case BookingStatus.Confirmed:
                // Can transition to InProgress or Completed
                if (newStatus == BookingStatus.InProgress || newStatus == BookingStatus.Completed)
                {
                    return (true, "");
                }
                return (false, $"Cannot transition from {currentStatus} to {newStatus}");

            case BookingStatus.InProgress:
                // Can only transition to Completed
                if (newStatus == BookingStatus.Completed)
                {
                    return (true, "");
                }
                return (false, "Booking in progress can only be marked as completed");

            case BookingStatus.Completed:
                return (false, "Cannot update a completed booking");

            case BookingStatus.Cancelled:
                return (false, "Cannot update a cancelled booking");

            default:
                return (false, $"Invalid current status: {currentStatus}");
        }
    }

    private string GetStatusDisplayName(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => "Pending",
            BookingStatus.Confirmed => "Confirmed",
            BookingStatus.InProgress => "In Progress",
            BookingStatus.Completed => "Completed",
            BookingStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}
