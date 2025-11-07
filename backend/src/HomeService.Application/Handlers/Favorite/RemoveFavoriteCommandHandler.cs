using HomeService.Application.Commands.Favorite;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Favorite;

public class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, Result<bool>>
{
    // TODO: Uncomment when Favorite entity is created
    // private readonly IRepository<Domain.Entities.Favorite> _favoriteRepository;
    // private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFavoriteCommandHandler> _logger;

    public RemoveFavoriteCommandHandler(
        // IRepository<Domain.Entities.Favorite> favoriteRepository,
        // IUnitOfWork unitOfWork,
        ILogger<RemoveFavoriteCommandHandler> logger)
    {
        // _favoriteRepository = favoriteRepository;
        // _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when Favorite entity exists
            /*
            // Find favorite
            var favorites = await _favoriteRepository.FindAsync(
                f => f.UserId == request.UserId && f.ServiceId == request.ServiceId,
                cancellationToken);

            var favorite = favorites?.FirstOrDefault();
            if (favorite == null)
            {
                return Result<bool>.Failure("Service not in favorites");
            }

            // Remove favorite
            await _favoriteRepository.DeleteAsync(favorite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service {ServiceId} removed from favorites by user {UserId}",
                request.ServiceId, request.UserId);

            return Result<bool>.Success(true, "Service removed from favorites");
            */

            // Temporary: Return success until entity is implemented
            _logger.LogWarning("Favorite entity not yet implemented. Cannot remove service {ServiceId} from favorites for user {UserId}",
                request.ServiceId, request.UserId);

            return Result<bool>.Success(true, "Favorites system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing service {ServiceId} from favorites for user {UserId}",
                request.ServiceId, request.UserId);
            return Result<bool>.Failure("An error occurred while removing from favorites");
        }
    }
}
