using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Bookings;

public record RejectBookingCommand(
    Guid BookingId,
    Guid ProviderId,
    string Reason
) : IRequest<Result<bool>>;

public class RejectBookingCommandHandler : IRequestHandler<RejectBookingCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RejectBookingCommandHandler> _logger;

    public RejectBookingCommandHandler(
        IUnitOfWork _unitOfWork,
        INotificationService notificationService,
        ILogger<RejectBookingCommandHandler> logger)
    {
        this._unitOfWork = _unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RejectBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<bool>("Booking not found");

            // Validate booking status
            if (booking.Status != BookingStatus.Pending)
                return Result.Failure<bool>(
                    $"Cannot reject booking with status {booking.Status}. Only Pending bookings can be rejected.");

            // Update booking to Cancelled
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = $"Rejected by provider: {request.Reason}";
            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Cancelled,
                ChangedById = request.ProviderId,
                Notes = $"Booking rejected by provider. Reason: {request.Reason}"
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
                        "Cancelled",
                        booking.Service.NameEn,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending booking rejection notification");
                }
            }, cancellationToken);

            _logger.LogInformation("Booking {BookingId} rejected by provider {ProviderId}. Reason: {Reason}",
                request.BookingId, request.ProviderId, request.Reason);

            return Result.Success(true, "Booking rejected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting booking {BookingId}", request.BookingId);
            return Result.Failure<bool>("An error occurred while rejecting the booking", ex.Message);
        }
    }
}
