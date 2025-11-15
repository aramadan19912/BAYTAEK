using Hangfire;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.BackgroundJobs;

public class HangfireJobScheduler
{
    private readonly ILogger<HangfireJobScheduler> _logger;

    public HangfireJobScheduler(ILogger<HangfireJobScheduler> logger)
    {
        _logger = logger;
    }

    public void ScheduleRecurringJobs()
    {
        try
        {
            _logger.LogInformation("Scheduling recurring Hangfire jobs");

            // Booking Reminders - Run every hour
            RecurringJob.AddOrUpdate<BookingReminderJob>(
                "booking-reminders",
                job => job.SendUpcomingBookingRemindersAsync(),
                Cron.Hourly);

            // Review Requests - Run every 6 hours
            RecurringJob.AddOrUpdate<ReviewRequestJob>(
                "review-requests",
                job => job.SendReviewRequestsAsync(),
                "0 */6 * * *"); // Every 6 hours

            // Auto-Create Payouts - Run daily at 2 AM
            RecurringJob.AddOrUpdate<PayoutProcessingJob>(
                "auto-create-payouts",
                job => job.AutoCreatePayoutsForProvidersAsync(),
                Cron.Daily(2));

            // Process Scheduled Payouts - Run daily at 3 AM
            RecurringJob.AddOrUpdate<PayoutProcessingJob>(
                "process-scheduled-payouts",
                job => job.ProcessScheduledPayoutsAsync(),
                Cron.Daily(3));

            // Cleanup Old Notifications - Run weekly on Sunday at 1 AM
            RecurringJob.AddOrUpdate<DataCleanupJob>(
                "cleanup-old-notifications",
                job => job.CleanupOldNotificationsAsync(),
                Cron.Weekly(DayOfWeek.Sunday, 1));

            // Cleanup Expired Refresh Tokens - Run daily at 4 AM
            RecurringJob.AddOrUpdate<DataCleanupJob>(
                "cleanup-expired-tokens",
                job => job.CleanupExpiredRefreshTokensAsync(),
                Cron.Daily(4));

            // Cleanup Old Blocked Dates - Run weekly on Sunday at 2 AM
            RecurringJob.AddOrUpdate<DataCleanupJob>(
                "cleanup-old-blocked-dates",
                job => job.CleanupOldBlockedDatesAsync(),
                Cron.Weekly(DayOfWeek.Sunday, 2));

            // Archive Old Bookings - Run monthly on the 1st at 3 AM
            RecurringJob.AddOrUpdate<DataCleanupJob>(
                "archive-old-bookings",
                job => job.ArchiveOldBookingsAsync(),
                Cron.Monthly(1, 3));

            // Cleanup Incomplete Bookings - Run daily at 5 AM
            RecurringJob.AddOrUpdate<DataCleanupJob>(
                "cleanup-incomplete-bookings",
                job => job.CleanupIncompleteBookingsAsync(),
                Cron.Daily(5));

            _logger.LogInformation("Successfully scheduled {Count} recurring Hangfire jobs", 9);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling recurring Hangfire jobs");
            throw;
        }
    }

    public void RemoveAllRecurringJobs()
    {
        _logger.LogInformation("Removing all recurring Hangfire jobs");

        RecurringJob.RemoveIfExists("booking-reminders");
        RecurringJob.RemoveIfExists("review-requests");
        RecurringJob.RemoveIfExists("auto-create-payouts");
        RecurringJob.RemoveIfExists("process-scheduled-payouts");
        RecurringJob.RemoveIfExists("cleanup-old-notifications");
        RecurringJob.RemoveIfExists("cleanup-expired-tokens");
        RecurringJob.RemoveIfExists("cleanup-old-blocked-dates");
        RecurringJob.RemoveIfExists("archive-old-bookings");
        RecurringJob.RemoveIfExists("cleanup-incomplete-bookings");

        _logger.LogInformation("All recurring Hangfire jobs removed");
    }
}
