using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.BackgroundJobs;

public class ReviewRequestJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReviewRequestJob> _logger;

    public ReviewRequestJob(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<ReviewRequestJob> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task SendReviewRequestsAsync()
    {
        try
        {
            _logger.LogInformation("Starting review request job");

            var now = DateTime.UtcNow;
            var reviewRequestWindow = now.AddHours(-24); // 24 hours after completion

            // Get completed bookings that don't have reviews yet
            var bookingsNeedingReview = await _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Include(b => b.Customer)
                .Include(b => b.Provider)
                    .ThenInclude(p => p!.User)
                .Include(b => b.Service)
                .Where(b => b.Status == BookingStatus.Completed &&
                           b.CompletedAt.HasValue &&
                           b.CompletedAt.Value <= reviewRequestWindow &&
                           b.CompletedAt.Value >= now.AddDays(-7) && // Within last 7 days
                           !b.ReviewRequestSent &&
                           !_unitOfWork.Repository<Review>()
                               .GetQueryable()
                               .Any(r => r.BookingId == b.Id))
                .ToListAsync();

            _logger.LogInformation("Found {Count} bookings needing review requests", bookingsNeedingReview.Count);

            foreach (var booking in bookingsNeedingReview)
            {
                try
                {
                    var titleEn = "How was your service?";
                    var titleAr = "كيف كانت الخدمة؟";
                    var messageEn = $"We hope you enjoyed {booking.Service.NameEn}! Please take a moment to rate your experience and help others make informed decisions.";
                    var messageAr = $"نأمل أن تكون قد استمتعت بخدمة {booking.Service.NameAr}! يرجى تقييم تجربتك لمساعدة الآخرين على اتخاذ قرارات مستنيرة.";

                    await _notificationService.SendNotificationAsync(
                        booking.CustomerId,
                        titleEn,
                        titleAr,
                        messageEn,
                        messageAr,
                        NotificationCategory.Reminder,
                        booking.Id,
                        nameof(Booking),
                        $"/bookings/{booking.Id}/review",
                        CancellationToken.None);

                    // Mark review request as sent
                    booking.ReviewRequestSent = true;
                    _unitOfWork.Repository<Booking>().Update(booking);

                    _logger.LogInformation("Review request sent for booking {BookingId}", booking.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending review request for booking {BookingId}", booking.Id);
                }
            }

            if (bookingsNeedingReview.Any())
            {
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }

            _logger.LogInformation("Review request job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in review request job");
            throw;
        }
    }
}
