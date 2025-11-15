using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.BackgroundJobs;

public class BookingReminderJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BookingReminderJob> _logger;

    public BookingReminderJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<BookingReminderJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendUpcomingBookingRemindersAsync()
    {
        try
        {
            _logger.LogInformation("Starting booking reminder job");

            var now = DateTime.UtcNow;
            var tomorrow = now.AddHours(24);

            // Get bookings scheduled for tomorrow (within 24 hours)
            var upcomingBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Provider)
                    .ThenInclude(p => p!.User)
                .Include(b => b.Service)
                .Where(b => b.Status == BookingStatus.Confirmed &&
                           b.ScheduledAt > now &&
                           b.ScheduledAt <= tomorrow &&
                           !b.ReminderSent)
                .ToListAsync();

            _logger.LogInformation("Found {Count} bookings to remind", upcomingBookings.Count);

            foreach (var booking in upcomingBookings)
            {
                try
                {
                    // Send reminder to customer
                    var hoursUntil = (booking.ScheduledAt - now).TotalHours;
                    var titleEn = "Upcoming Booking Reminder";
                    var titleAr = "تذكير بالحجز القادم";
                    var messageEn = $"Your booking for {booking.Service.NameEn} is scheduled in {Math.Round(hoursUntil)} hours on {booking.ScheduledAt:yyyy-MM-dd HH:mm}.";
                    var messageAr = $"حجزك لخدمة {booking.Service.NameAr} مجدول خلال {Math.Round(hoursUntil)} ساعة في {booking.ScheduledAt:yyyy-MM-dd HH:mm}.";

                    await _notificationService.SendNotificationAsync(
                        booking.CustomerId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Reminder,
                        booking.Id,
                        nameof(Booking),
                        $"/bookings/{booking.Id}",
                        CancellationToken.None);

                    // Send reminder to provider if assigned
                    if (booking.ProviderId.HasValue && booking.Provider != null)
                    {
                        var providerMessageEn = $"Reminder: You have a booking for {booking.Service.NameEn} in {Math.Round(hoursUntil)} hours on {booking.ScheduledAt:yyyy-MM-dd HH:mm}.";
                        var providerMessageAr = $"تذكير: لديك حجز لخدمة {booking.Service.NameAr} خلال {Math.Round(hoursUntil)} ساعة في {booking.ScheduledAt:yyyy-MM-dd HH:mm}.";

                        await _notificationService.SendNotificationAsync(
                            booking.Provider.UserId,
                            titleEn,
                            titleAr,
                            providerMessageEn,
                            providerMessageAr,
                            NotificationCategory.Reminder,
                            booking.Id,
                            nameof(Booking),
                            $"/provider/bookings/{booking.Id}",
                            CancellationToken.None);
                    }

                    // Mark reminder as sent
                    booking.ReminderSent = true;
                    _unitOfWork.Repository<Booking>().Update(booking);

                    _logger.LogInformation("Reminder sent for booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder for booking {BookingId}", booking.Id);
                }
            }

            if (upcomingBookings.Any())
            {
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }

            _logger.LogInformation("Booking reminder job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in booking reminder job");
            throw;
        }
    }
}
