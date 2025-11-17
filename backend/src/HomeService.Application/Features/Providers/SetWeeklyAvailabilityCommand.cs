using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record SetWeeklyAvailabilityCommand(
    Guid ProviderId,
    List<WeeklyScheduleDto> Schedule
) : IRequest<Result<bool>>;

public class WeeklyScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class SetWeeklyAvailabilityCommandHandler
    : IRequestHandler<SetWeeklyAvailabilityCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetWeeklyAvailabilityCommandHandler> _logger;

    public SetWeeklyAvailabilityCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SetWeeklyAvailabilityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        SetWeeklyAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<bool>("Provider not found");

            // Validate schedule
            foreach (var schedule in request.Schedule)
            {
                if (schedule.StartTime >= schedule.EndTime)
                    return Result.Failure<bool>(
                        $"Invalid time range for {schedule.DayOfWeek}: Start time must be before end time");

                if (schedule.StartTime < TimeSpan.Zero || schedule.EndTime > TimeSpan.FromHours(24))
                    return Result.Failure<bool>(
                        $"Invalid time range for {schedule.DayOfWeek}: Times must be between 00:00 and 24:00");
            }

            // Get existing availability records
            var existingAvailability = await _unitOfWork.Repository<ProviderAvailability>()
                .GetQueryable()
                .Where(a => a.ProviderId == request.ProviderId)
                .ToListAsync(cancellationToken);

            // Remove all existing records
            foreach (var existing in existingAvailability)
            {
                _unitOfWork.Repository<ProviderAvailability>().Delete(existing);
            }

            // Add new schedule
            foreach (var schedule in request.Schedule)
            {
                var availability = new ProviderAvailability
                {
                    ProviderId = request.ProviderId,
                    DayOfWeek = schedule.DayOfWeek,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    IsAvailable = schedule.IsAvailable,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ProviderId.ToString()
                };

                await _unitOfWork.Repository<ProviderAvailability>()
                    .AddAsync(availability, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Weekly availability set for provider {ProviderId}: {ScheduleCount} time slots",
                request.ProviderId, request.Schedule.Count);

            return Result.Success(true, "Weekly availability updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting weekly availability for provider {ProviderId}",
                request.ProviderId);
            return Result.Failure<bool>(
                "An error occurred while updating availability",
                ex.Message);
        }
    }
}
