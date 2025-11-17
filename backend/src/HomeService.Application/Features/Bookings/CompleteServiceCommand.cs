using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Bookings;

public record CompleteServiceCommand(
    Guid BookingId,
    Guid ProviderId,
    List<string>? CompletionPhotoUrls = null,
    string? Notes = null
) : IRequest<Result<BookingDto>>;

public class CompleteServiceCommandHandler : IRequestHandler<CompleteServiceCommand, Result<BookingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CompleteServiceCommandHandler> _logger;

    public CompleteServiceCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<CompleteServiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(CompleteServiceCommand request, CancellationToken cancellationToken)
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
            if (booking.Status != BookingStatus.InProgress)
                return Result.Failure<BookingDto>(
                    $"Cannot complete booking with status {booking.Status}. Only In Progress bookings can be completed.");

            // Validate that service was actually started
            if (!booking.StartedAt.HasValue)
                return Result.Failure<BookingDto>("Cannot complete booking that was not started");

            // Update booking
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;

            // Store completion photos if provided
            if (request.CompletionPhotoUrls != null && request.CompletionPhotoUrls.Any())
            {
                booking.CompletionPhotos = string.Join(",", request.CompletionPhotoUrls);
            }

            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var historyNotes = "Service completed";
            if (!string.IsNullOrEmpty(request.Notes))
            {
                historyNotes += $". Notes: {request.Notes}";
            }
            if (request.CompletionPhotoUrls != null && request.CompletionPhotoUrls.Any())
            {
                historyNotes += $". {request.CompletionPhotoUrls.Count} photo(s) uploaded";
            }

            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Completed,
                ChangedBy = request.ProviderId,
                Notes = historyNotes
            };
            await _unitOfWork.Repository<BookingHistory>().AddAsync(history, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notifications to customer
            _ = Task.Run(async () =>
            {
                try
                {
                    // Service completion notification
                    await _notificationService.SendBookingStatusUpdateAsync(
                        booking.CustomerId,
                        booking.Id,
                        "Completed",
                        booking.Service.NameEn,
                        CancellationToken.None);

                    // Review request notification (delayed by 1 hour)
                    await Task.Delay(TimeSpan.FromHours(1));
                    await _notificationService.SendReviewRequestAsync(
                        booking.CustomerId,
                        booking.Id,
                        booking.Service.NameEn,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending service completion notifications");
                }
            }, cancellationToken);

            var duration = booking.CompletedAt.Value - booking.StartedAt.Value;
            _logger.LogInformation(
                "Service completed for booking {BookingId} by provider {ProviderId}. Duration: {Duration} minutes",
                request.BookingId, request.ProviderId, duration.TotalMinutes);

            var bookingDto = MapToDto(booking);
            return Result.Success(bookingDto, "Service completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing service for booking {BookingId}", request.BookingId);
            return Result.Failure<BookingDto>("An error occurred while completing the service", ex.Message);
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
