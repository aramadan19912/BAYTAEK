using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record GetProviderPerformanceQuery(
    Guid ProviderId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<ProviderPerformanceResponse>>;

public class GetProviderPerformanceQueryHandler
    : IRequestHandler<GetProviderPerformanceQuery, Result<ProviderPerformanceResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderPerformanceQueryHandler> _logger;

    public GetProviderPerformanceQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderPerformanceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProviderPerformanceResponse>> Handle(
        GetProviderPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ProviderPerformanceResponse>("Provider not found");

            // Get all bookings for date range
            var bookingsQuery = _unitOfWork.Repository<Booking>()
                .GetQueryable()
                .Where(b => b.ProviderId == request.ProviderId);

            if (request.StartDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= request.EndDate.Value);

            var bookings = await bookingsQuery.ToListAsync(cancellationToken);

            // Calculate metrics
            var totalBookings = bookings.Count;
            var completedBookings = bookings.Count(b => b.Status == BookingStatus.Completed);
            var cancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled);
            var rejectedBookings = bookings.Count(b => b.Status == BookingStatus.Rejected);

            var completionRate = totalBookings > 0
                ? Math.Round((double)completedBookings / totalBookings * 100, 2)
                : 0;

            var cancellationRate = totalBookings > 0
                ? Math.Round((double)cancelledBookings / totalBookings * 100, 2)
                : 0;

            var acceptanceRate = totalBookings > 0
                ? Math.Round((double)(totalBookings - rejectedBookings) / totalBookings * 100, 2)
                : 0;

            // Get reviews
            var reviews = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Where(r => r.ProviderId == request.ProviderId && r.IsVisible)
                .ToListAsync(cancellationToken);

            var averageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 2) : 0;
            var totalReviews = reviews.Count;

            // Calculate response time (time to accept booking)
            var acceptedBookings = bookings
                .Where(b => b.Status != BookingStatus.Pending && b.UpdatedAt.HasValue)
                .ToList();

            var averageResponseTimeMinutes = acceptedBookings.Any()
                ? acceptedBookings.Average(b => (b.UpdatedAt!.Value - b.CreatedAt).TotalMinutes)
                : 0;

            // Calculate average service duration
            var completedWithDuration = bookings
                .Where(b => b.Status == BookingStatus.Completed &&
                           b.StartedAt.HasValue &&
                           b.CompletedAt.HasValue)
                .ToList();

            var averageServiceDurationMinutes = completedWithDuration.Any()
                ? completedWithDuration.Average(b =>
                    (b.CompletedAt!.Value - b.StartedAt!.Value).TotalMinutes)
                : 0;

            // Service breakdown
            var serviceBreakdown = bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => b.ServiceId)
                .Select(g => new ServicePerformanceDto
                {
                    ServiceId = g.Key,
                    BookingsCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(s => s.BookingsCount)
                .ToList();

            // Get service names
            var serviceIds = serviceBreakdown.Select(s => s.ServiceId).ToList();
            var services = await _unitOfWork.Repository<Service>()
                .GetQueryable()
                .Where(s => serviceIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            foreach (var service in serviceBreakdown)
            {
                var serviceEntity = services.FirstOrDefault(s => s.Id == service.ServiceId);
                if (serviceEntity != null)
                {
                    service.ServiceName = serviceEntity.NameEn;
                }
            }

            var response = new ProviderPerformanceResponse
            {
                ProviderId = request.ProviderId,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                RejectedBookings = rejectedBookings,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate,
                AcceptanceRate = acceptanceRate,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                AverageResponseTimeMinutes = Math.Round(averageResponseTimeMinutes, 2),
                AverageServiceDurationMinutes = Math.Round(averageServiceDurationMinutes, 2),
                ServiceBreakdown = serviceBreakdown,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance for provider {ProviderId}",
                request.ProviderId);
            return Result.Failure<ProviderPerformanceResponse>(
                "An error occurred while retrieving performance metrics",
                ex.Message);
        }
    }
}

public class ProviderPerformanceResponse
{
    public Guid ProviderId { get; set; }

    // Booking metrics
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int RejectedBookings { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public double AcceptanceRate { get; set; }

    // Rating metrics
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Time metrics
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageServiceDurationMinutes { get; set; }

    // Service breakdown
    public List<ServicePerformanceDto> ServiceBreakdown { get; set; } = new();

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ServicePerformanceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
