using HomeService.Application.Commands.Bookings;
using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Bookings;

public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, Result>
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBookingStatusCommandHandler> _logger;

    public UpdateBookingStatusCommandHandler(
        IRepository<Booking> bookingRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBookingStatusCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result.Failure("Booking not found");
            }

            // Verify user has permission to update booking
            if (booking.CustomerId != request.UserId && booking.ProviderId != request.UserId)
            {
                return Result.Failure("Unauthorized to update this booking");
            }

            // Validate status transition
            if (!IsValidStatusTransition(booking.Status, request.NewStatus))
            {
                return Result.Failure($"Invalid status transition from {booking.Status} to {request.NewStatus}");
            }

            booking.Status = request.NewStatus;

            switch (request.NewStatus)
            {
                case BookingStatus.InProgress:
                    booking.StartedAt = DateTime.UtcNow;
                    break;
                case BookingStatus.Completed:
                    booking.CompletedAt = DateTime.UtcNow;
                    break;
                case BookingStatus.Cancelled:
                    booking.CancelledAt = DateTime.UtcNow;
                    booking.CancellationReason = request.CancellationReason;
                    break;
            }

            await _bookingRepository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Booking {BookingId} status updated to {NewStatus}", request.BookingId, request.NewStatus);

            return Result.Success($"Booking status updated to {request.NewStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking status");
            return Result.Failure("An error occurred while updating booking status", ex.Message);
        }
    }

    private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.InProgress) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.InProgress, BookingStatus.Completed) => true,
            (BookingStatus.InProgress, BookingStatus.Disputed) => true,
            (BookingStatus.Completed, BookingStatus.Disputed) => true,
            _ => false
        };
    }
}
