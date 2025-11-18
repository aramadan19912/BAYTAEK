using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Auth;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result<bool>>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IRepository<Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate password strength
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            {
                return Result<bool>.Failure("Password must be at least 8 characters long");
            }

            // Find user by email
            var users = await _userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            var user = users?.FirstOrDefault();

            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                // Don't reveal if user exists
                return Result<bool>.Failure("Invalid reset token or email");
            }

            // TODO: Validate reset token
            // In a production system, you would:
            // 1. Check if token exists and matches the hashed token in database
            // 2. Check if token has expired (e.g., 1 hour expiry)
            // 3. Mark token as used to prevent reuse

            // For now, we'll implement a simplified version
            // This should be enhanced with proper token storage and validation

            // Hash the new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Update user password
            user.PasswordHash = hashedPassword;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset successful for user {Email}", request.Email);

            return Result<bool>.Success(true, "Password reset successfully. You can now log in with your new password.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
            return Result<bool>.Failure("An error occurred while resetting your password");
        }
    }
}
