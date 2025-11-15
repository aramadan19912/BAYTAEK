using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Bookings;

public record StartServiceCommand(
    Guid BookingId,
    Guid ProviderId,
    double? Latitude = null,
    double? Longitude = null
) : IRequest<Result<BookingDto>>;

public class StartServiceCommandHandler : IRequestHandler<StartServiceCommand, Result<BookingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StartServiceCommandHandler> _logger;

    public StartServiceCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<StartServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(StartServiceCommand request, CancellationToken cancellationToken)
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

            // Validate provider
            if (booking.ProviderId != request.ProviderId)
                return Result.Failure<BookingDto>("Unauthorized: You are not assigned to this booking");

            // Validate booking status
            if (booking.Status != BookingStatus.Confirmed)
                return Result.Failure<BookingDto>(
                    $"Cannot start service for booking with status {booking.Status}. Only Confirmed bookings can be started.");

            // Check if booking is scheduled for today or earlier
            if (booking.ScheduledAt.Date > DateTime.UtcNow.Date)
                return Result.Failure<BookingDto>(
                    "Cannot start service before scheduled date");

            // Update booking
            booking.Status = BookingStatus.InProgress;
            booking.StartedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var historyNotes = "Service started";
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                historyNotes += $" at location ({request.Latitude}, {request.Longitude})";
            }

            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.InProgress,
                ChangedBy = request.ProviderId,
                Notes = historyNotes
            };
            await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to customer
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.SendBookingStatusUpdateAsync(
                        booking.CustomerId,
                        booking.Id,
                        "In Progress",
                        booking.Service.NameEn,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending service started notification");
                }
            }, cancellationToken);

            _logger.LogInformation("Service started for booking {BookingId} by provider {ProviderId}",
                request.BookingId, request.ProviderId);

            var bookingDto = MapToDto(booking);
            return Result.Success(bookingDto, "Service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting service for booking {BookingId}", request.BookingId);
            return Result.Failure<BookingDto>("An error occurred while starting the service", ex.Message);
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
