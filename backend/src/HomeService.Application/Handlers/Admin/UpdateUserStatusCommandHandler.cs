using HomeService.Application.Commands.Admin;
using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserStatusCommandHandler> _logger;

    public UpdateUserStatusCommandHandler(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserStatusCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.IsDeleted = request.IsSuspended;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = request.AdminUserId.ToString();

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var action = request.IsSuspended ? "suspended" : "activated";
            _logger.LogInformation("User {UserId} has been {Action} by admin {AdminId}. Reason: {Reason}",
                request.UserId, action, request.AdminUserId, request.Reason);

            return Result.Success($"User {action} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status");
            return Result.Failure("Error updating user status", ex.Message);
        }
    }
}
