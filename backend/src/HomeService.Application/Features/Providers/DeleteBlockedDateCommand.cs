using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record DeleteBlockedDateCommand(
    Guid BlockedDateId,
    Guid ProviderId
) : IRequest<Result<bool>>;

public class DeleteBlockedDateCommandHandler
    : IRequestHandler<DeleteBlockedDateCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBlockedDateCommandHandler> _logger;

    public DeleteBlockedDateCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteBlockedDateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteBlockedDateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var blockedDate = await _unitOfWork.Repository<ProviderBlockedDate>()
                .GetByIdAsync(request.BlockedDateId, cancellationToken);

            if (blockedDate == null)
                return Result.Failure<bool>("Blocked date not found");

            // Validate ownership
            if (blockedDate.ProviderId != request.ProviderId)
                return Result.Failure<bool>("Unauthorized: You cannot delete this blocked date");

            _unitOfWork.Repository<ProviderBlockedDate>().Delete(blockedDate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Blocked date {BlockedDateId} deleted by provider {ProviderId}",
                request.BlockedDateId, request.ProviderId);

            return Result.Success(true, "Blocked date removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blocked date {BlockedDateId}", request.BlockedDateId);
            return Result.Failure<bool>(
                "An error occurred while removing the blocked date",
                ex.Message);
        }
    }
}
