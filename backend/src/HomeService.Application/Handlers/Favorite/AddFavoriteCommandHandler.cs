using HomeService.Application.Commands.Favorite;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Favorite;

public class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Result<bool>>
{
    // TODO: Uncomment when Favorite entity is created
    // private readonly IRepository<Domain.Entities.Favorite> _favoriteRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    // private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddFavoriteCommandHandler> _logger;

    public AddFavoriteCommandHandler(
        // IRepository<Domain.Entities.Favorite> favoriteRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        // IUnitOfWork unitOfWork,
        ILogger<AddFavoriteCommandHandler> logger)
    {
        // _favoriteRepository = favoriteRepository;
        _serviceRepository = serviceRepository;
        // _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify service exists
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result<bool>.Failure("Service not found");
            }

            // TODO: Implement when Favorite entity exists
            /*
            // Check if already favorited
            var existingFavorite = await _favoriteRepository.FindAsync(
                f => f.UserId == request.UserId && f.ServiceId == request.ServiceId,
                cancellationToken);

            if (existingFavorite != null && existingFavorite.Any())
            {
                return Result<bool>.Success(true, "Service already in favorites");
            }

            // Create favorite
            var favorite = new Domain.Entities.Favorite
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ServiceId = request.ServiceId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId.ToString()
            };

            await _favoriteRepository.AddAsync(favorite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service {ServiceId} added to favorites by user {UserId}",
                request.ServiceId, request.UserId);

            return Result<bool>.Success(true, "Service added to favorites");
            */

            // Temporary: Return success until entity is implemented
            _logger.LogWarning("Favorite entity not yet implemented. Cannot add service {ServiceId} to favorites for user {UserId}",
                request.ServiceId, request.UserId);

            return Result<bool>.Success(true, "Favorites system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service {ServiceId} to favorites for user {UserId}",
                request.ServiceId, request.UserId);
            return Result<bool>.Failure("An error occurred while adding to favorites");
        }
    }
}
