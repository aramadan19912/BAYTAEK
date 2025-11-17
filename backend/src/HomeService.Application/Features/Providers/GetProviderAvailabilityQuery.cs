using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Providers;

public record GetProviderAvailabilityQuery(
    Guid ProviderId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<ProviderAvailabilityResponse>>;

public class GetProviderAvailabilityQueryHandler
    : IRequestHandler<GetProviderAvailabilityQuery, Result<ProviderAvailabilityResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProviderAvailabilityQueryHandler> _logger;

    public GetProviderAvailabilityQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetProviderAvailabilityQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ProviderAvailabilityResponse>> Handle(
        GetProviderAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ProviderAvailabilityResponse>("Provider not found");

            // Get weekly schedule
            var weeklySchedule = await _unitOfWork.Repository<ProviderAvailability>()
                .GetQueryable()
                .Where(a => a.ProviderId == request.ProviderId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync(cancellationToken);

            // Get blocked dates
            var blockedDatesQuery = _unitOfWork.Repository<ProviderBlockedDate>()
                .GetQueryable()
                .Where(bd => bd.ProviderId == request.ProviderId);

            if (request.StartDate.HasValue)
                blockedDatesQuery = blockedDatesQuery.Where(bd => bd.EndDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                blockedDatesQuery = blockedDatesQuery.Where(bd => bd.StartDate <= request.EndDate.Value);

            var blockedDates = await blockedDatesQuery
                .OrderBy(bd => bd.StartDate)
                .ToListAsync(cancellationToken);

            var response = new ProviderAvailabilityResponse
            {
                ProviderId = request.ProviderId,
                WeeklySchedule = weeklySchedule.Select(ws => new WeeklyScheduleDto
                {
                    DayOfWeek = ws.DayOfWeek,
                    StartTime = ws.StartTime,
                    EndTime = ws.EndTime,
                    IsAvailable = ws.IsAvailable
                }).ToList(),
                BlockedDates = blockedDates.Select(bd => new BlockedDateDto
                {
                    Id = bd.Id,
                    ProviderId = bd.ProviderId,
                    StartDate = bd.StartDate,
                    EndDate = bd.EndDate,
                    Reason = bd.Reason,
                    IsAllDay = bd.IsAllDay,
                    CreatedAt = bd.CreatedAt
                }).ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability for provider {ProviderId}", request.ProviderId);
            return Result.Failure<ProviderAvailabilityResponse>(
                "An error occurred while retrieving availability",
                ex.Message);
        }
    }
}

public class ProviderAvailabilityResponse
{
    public Guid ProviderId { get; set; }
    public List<WeeklyScheduleDto> WeeklySchedule { get; set; } = new();
    public List<BlockedDateDto> BlockedDates { get; set; } = new();
}
