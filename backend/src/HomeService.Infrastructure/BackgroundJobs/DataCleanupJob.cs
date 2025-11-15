using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.BackgroundJobs;

public class DataCleanupJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataCleanupJob> _logger;

    public DataCleanupJob(
        IUnitOfWork unitOfWork,
        ILogger<DataCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CleanupOldNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("Starting old notifications cleanup job");

            var cutoffDate = DateTime.UtcNow.AddDays(-90); // Keep last 90 days

            var oldNotifications = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                foreach (var notification in oldNotifications)
                {
                    _unitOfWork.Repository<Notification>().Delete(notification);
                }

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Deleted {Count} old notifications", oldNotifications.Count);
            }
            else
            {
                _logger.LogInformation("No old notifications to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in old notifications cleanup job");
            throw;
        }
    }

    public async Task CleanupExpiredRefreshTokensAsync()
    {
        try
        {
            _logger.LogInformation("Starting expired refresh tokens cleanup job");

            var cutoffDate = DateTime.UtcNow;

            var expiredTokens = await _unitOfWork.Repository<RefreshToken>()
                .GetQueryable()
                .Where(t => t.ExpiresAt < cutoffDate || t.RevokedAt != null)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                foreach (var token in expiredTokens)
                {
                    _unitOfWork.Repository<RefreshToken>().Delete(token);
                }

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Deleted {Count} expired/revoked refresh tokens", expiredTokens.Count);
            }
            else
            {
                _logger.LogInformation("No expired refresh tokens to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in expired refresh tokens cleanup job");
            throw;
        }
    }

    public async Task CleanupOldBlockedDatesAsync()
    {
        try
        {
            _logger.LogInformation("Starting old blocked dates cleanup job");

            var now = DateTime.UtcNow;

            var oldBlockedDates = await _unitOfWork.Repository<ProviderBlockedDate>()
                .GetQueryable()
                .Where(bd => bd.EndDate < now.AddDays(-30)) // 30 days after end date
                .ToListAsync();

            if (oldBlockedDates.Any())
            {
                foreach (var blockedDate in oldBlockedDates)
                {
                    _unitOfWork.Repository<ProviderBlockedDate>().Delete(blockedDate);
                }

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Deleted {Count} old blocked dates", oldBlockedDates.Count);
            }
            else
            {
                _logger.LogInformation("No old blocked dates to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in old blocked dates cleanup job");
            throw;
        }
    }

    public async Task ArchiveOldBookingsAsync()
    {
        try
        {
            _logger.LogInformation("Starting old bookings archival job");

            var cutoffDate = DateTime.UtcNow.AddYears(-2); // Archive bookings older than 2 years

            var oldBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b => b.CreatedAt < cutoffDate &&
                           (b.Status == Domain.Enums.BookingStatus.Completed ||
                            b.Status == Domain.Enums.BookingStatus.Cancelled))
                .ToListAsync();

            if (oldBookings.Any())
            {
                // In a real scenario, you might move these to an archive database or cold storage
                // For now, we'll just log the count
                _logger.LogInformation("Found {Count} old bookings to archive (keeping in database for now)", oldBookings.Count);
            }
            else
            {
                _logger.LogInformation("No old bookings to archive");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in old bookings archival job");
            throw;
        }
    }

    public async Task CleanupIncompleteBookingsAsync()
    {
        try
        {
            _logger.LogInformation("Starting incomplete bookings cleanup job");

            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            // Find pending bookings that are very old and never confirmed
            var staleBookings = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b => b.Status == Domain.Enums.BookingStatus.Pending &&
                           b.CreatedAt < cutoffDate)
                .ToListAsync();

            if (staleBookings.Any())
            {
                foreach (var booking in staleBookings)
                {
                    booking.Status = Domain.Enums.BookingStatus.Cancelled;
                    booking.UpdatedAt = DateTime.UtcNow;
                    booking.UpdatedBy = "System";
                    _unitOfWork.Repository<Booking>().Update(booking);
                }

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation("Cancelled {Count} stale pending bookings", staleBookings.Count);
            }
            else
            {
                _logger.LogInformation("No stale bookings to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in incomplete bookings cleanup job");
            throw;
        }
    }
}
