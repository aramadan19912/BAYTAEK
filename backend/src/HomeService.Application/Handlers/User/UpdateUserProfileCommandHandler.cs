using HomeService.Application.Commands.User;
using HomeService.Domain.Interfaces;
using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Enums;

namespace HomeService.Application.Handlers.User;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IRepository<Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<bool>("User not found");
            }

            // Track changes
            var hasChanges = false;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != user.FirstName)
            {
                user.FirstName = request.FirstName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != user.LastName)
            {
                user.LastName = request.LastName.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            {
                // TODO: Validate phone number format
                // TODO: If phone number changes, should reset PhoneVerified to false
                user.PhoneNumber = request.PhoneNumber.Trim();
                user.IsPhoneVerified = false; // Require re-verification
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
            {
                // Parse and validate language enum
                if (Enum.TryParse<Language>(request.PreferredLanguage, true, out var language) && language != user.PreferredLanguage)
                {
                    user.PreferredLanguage = language;
                    hasChanges = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl) && request.ProfileImageUrl != user.ProfileImageUrl)
            {
                user.ProfileImageUrl = request.ProfileImageUrl;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return Result.Success(true, "No changes detected");
            }

            // Update timestamps
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = request.UserId.ToString();

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated for user {UserId}", request.UserId);

            return Result.Success(true, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
            return Result.Failure<bool>("An error occurred while updating the profile");
        }
    }
}
