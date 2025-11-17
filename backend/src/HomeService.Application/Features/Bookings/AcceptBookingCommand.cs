using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Bookings;

public record AcceptBookingCommand(Guid BookingId, Guid ProviderId, int? EstimatedDurationMinutes = null)
    : IRequest<Result<BookingDto>>;

public class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, Result<BookingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AcceptBookingCommandHandler> _logger;

    public AcceptBookingCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<AcceptBookingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking with related data
            var booking = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Address)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
                return Result.Failure<BookingDto>("Booking not found");

            // Validate booking status
            if (booking.Status != BookingStatus.Pending)
                return Result.Failure<BookingDto>(
                    $"Cannot accept booking with status {booking.Status}. Only Pending bookings can be accepted.");

            // Verify provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<BookingDto>("Provider not found");

            // Check if provider offers this service
            var providerService = await _unitOfWork.Repository<ProviderService>()
                .GetQueryable()
                .FirstOrDefaultAsync(ps => ps.ProviderId == request.ProviderId
                    && ps.ServiceId == booking.ServiceId, cancellationToken);

            if (providerService == null)
                return Result.Failure<BookingDto>("Provider does not offer this service");

            // Update booking
            booking.ProviderId = request.ProviderId;
            booking.Status = BookingStatus.Confirmed;
            _unitOfWork.Repository<Booking>().Update(booking);

            // Create history record
            var history = new BookingHistory
            {
                BookingId = booking.Id,
                Status = BookingStatus.Confirmed,
                ChangedBy = request.ProviderId,
                Notes = request.EstimatedDurationMinutes.HasValue
                    ? $"Booking accepted. Estimated duration: {request.EstimatedDurationMinutes} minutes"
                    : "Booking accepted by provider"
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
                        "Confirmed",
                        booking.Service.NameEn,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending booking accepted notification");
                }
            }, cancellationToken);

            _logger.LogInformation("Booking {BookingId} accepted by provider {ProviderId}",
                request.BookingId, request.ProviderId);

            var bookingDto = MapToDto(booking);
            return Result.Success(bookingDto, "Booking accepted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting booking {BookingId}", request.BookingId);
            return Result.Failure<BookingDto>("An error occurred while accepting the booking", ex.Message);
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
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt
        };
    }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? ProviderId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
