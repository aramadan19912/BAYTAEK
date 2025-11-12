using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record BlockDatesCommand(
    Guid ProviderId,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    bool IsAllDay = true
) : IRequest<Result<BlockedDateDto>>;

public class BlockDatesCommandHandler : IRequestHandler<BlockDatesCommand, Result<BlockedDateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlockDatesCommandHandler> _logger;

    public BlockDatesCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<BlockDatesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BlockedDateDto>> Handle(
        BlockDatesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<BlockedDateDto>("Provider not found");

            // Validate dates
            if (request.StartDate >= request.EndDate)
                return Result.Failure<BlockedDateDto>("Start date must be before end date");

            if (request.StartDate < DateTime.UtcNow)
                return Result.Failure<BlockedDateDto>("Cannot block dates in the past");

            // Create blocked date
            var blockedDate = new ProviderBlockedDate
            {
                ProviderId = request.ProviderId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                IsAllDay = request.IsAllDay,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ProviderId.ToString()
            };

            await _unitOfWork.Repository<ProviderBlockedDate>()
                .AddAsync(blockedDate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Dates blocked for provider {ProviderId}: {StartDate} to {EndDate}",
                request.ProviderId, request.StartDate, request.EndDate);

            var dto = new BlockedDateDto
            {
                Id = blockedDate.Id,
                ProviderId = blockedDate.ProviderId,
                StartDate = blockedDate.StartDate,
                EndDate = blockedDate.EndDate,
                Reason = blockedDate.Reason,
                IsAllDay = blockedDate.IsAllDay,
                CreatedAt = blockedDate.CreatedAt
            };

            return Result.Success(dto, "Dates blocked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking dates for provider {ProviderId}", request.ProviderId);
            return Result.Failure<BlockedDateDto>(
                "An error occurred while blocking dates",
                ex.Message);
        }
    }
}

public class BlockedDateDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsAllDay { get; set; }
    public DateTime CreatedAt { get; set; }
}
