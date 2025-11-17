using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Bookings;

public record RescheduleBookingCommand(
    Guid BookingId,
    Guid UserId,
    DateTime NewScheduledAt,
    string? Reason = null,
    bool IsCustomerRequest = true
) : IRequest<Result<BookingDto>>;

public class RescheduleBookingCommandHandler
    : IRequestHandler<RescheduleBookingCommand, Result<BookingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RescheduleBookingCommandHandler> _logger;

    public RescheduleBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<RescheduleBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(
        RescheduleBookingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Provider)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<BookingDto>("Booking not found");

            // Validate permissions
            if (request.IsCustomerRequest && booking.CustomerId != request.UserId)
                return Result.Failure<BookingDto>("Unauthorized: You cannot reschedule this booking");

            if (!request.IsCustomerRequest && booking.ProviderId != request.UserId)
                return Result.Failure<BookingDto>("Unauthorized: You cannot reschedule this booking");

            // Validate booking status - can only reschedule pending or confirmed bookings
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                return Result.Failure<BookingDto>(
                    $"Cannot reschedule booking with status {booking.Status}. " +
                    $"Only Pending or Confirmed bookings can be rescheduled.");

            // Validate new scheduled time is in the future
            if (request.NewScheduledAt <= DateTime.UtcNow)
                return Result.Failure<BookingDto>("New scheduled time must be in the future");

            // Validate not rescheduling to the same time
            if (request.NewScheduledAt == booking.ScheduledAt)
                return Result.Failure<BookingDto>("New scheduled time is the same as current time");

            // Check rescheduling restrictions (e.g., minimum notice period)
            var hoursUntilOriginal = (booking.ScheduledAt - DateTime.UtcNow).TotalHours;
            if (hoursUntilOriginal < 2)
                return Result.Failure<BookingDto>(
                    "Cannot reschedule less than 2 hours before the scheduled time");

            // Store old scheduled time
            var oldScheduledAt = booking.ScheduledAt;

            // Update booking
            booking.ScheduledAt = request.NewScheduledAt;
            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var historyNotes = $"Rescheduled by {(request.IsCustomerRequest ? "customer" : "provider")} " +
                              $"from {oldScheduledAt:yyyy-MM-dd HH:mm} to {request.NewScheduledAt:yyyy-MM-dd HH:mm}";

            if (!string.IsNullOrEmpty(request.Reason))
            {
                historyNotes += $". Reason: {request.Reason}";
            }

            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = booking.Status, // Status remains the same
                ChangedBy = request.UserId,
                Notes = historyNotes
            };
            await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to the other party
            _ = Task.Run(async () =>
            {
                try
                {
                    var notifyUserId = request.IsCustomerRequest
                        ? booking.ProviderId ?? Guid.Empty
                        : booking.CustomerId;

                    if (notifyUserId != Guid.Empty)
                    {
                        var titleEn = "Booking Rescheduled";
                        var titleAr = "تم إعادة جدولة الحجز";
                        var messageEn = $"Your booking for {booking.Service.NameEn} has been rescheduled " +
                                       $"to {request.NewScheduledAt:dddd, MMMM dd, yyyy 'at' hh:mm tt}";
                        var messageAr = $"تم إعادة جدولة حجزك لخدمة {booking.Service.NameAr} " +
                                       $"إلى {request.NewScheduledAt:dddd, dd MMMM yyyy 'الساعة' hh:mm tt}";

                        await _notificationService.SendNotificationAsync(
                            notifyUserId,
                            titleEn,
                            titleAr,
                            messageEn,
                            messageAr,
                            NotificationCategory.Booking,
                            booking.Id,
                            nameof(Booking),
                            $"/bookings/{booking.Id}",
                            CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending rescheduling notification");
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Booking {BookingId} rescheduled from {OldDate} to {NewDate} by {UserType}",
                request.BookingId,
                oldScheduledAt,
                request.NewScheduledAt,
                request.IsCustomerRequest ? "customer" : "provider");

            var bookingDto = MapToDto(booking);
            return Result.Success(bookingDto, "Booking rescheduled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling booking {BookingId}", request.BookingId);
            return Result.Failure<BookingDto>(
                "An error occurred while rescheduling the booking",
                ex.Message);
        }
    }

    private BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            ServiceId = booking.ServiceId,
            ProviderId = booking.ProviderId,
            Status = booking.Status,
            ScheduledAt = booking.ScheduledAt,
            StartedAt = booking.StartedAt,
            CompletedAt = booking.CompletedAt,
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt
        };
    }
}
