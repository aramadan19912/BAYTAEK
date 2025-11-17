using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;
using System.Security.Cryptography;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Auth;

public record ForgotPasswordCommand(string Email) : IRequest<Result<bool>>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IRepository<Domain.Entities.User> userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var users = await _userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            var user = users?.FirstOrDefault();

            // Don't reveal if user exists or not (security best practice)
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                // Return success anyway to prevent email enumeration
                return Result<bool>.Success(true, "If the email exists, a password reset link has been sent");
            }

            // Generate reset token
            var resetToken = GenerateResetToken();
            var resetTokenHash = HashToken(resetToken);

            // Store reset token in user record (you may want a separate PasswordReset table in production)
            // For now, we'll use a field in User entity or store in a temporary way
            // TODO: Add PasswordResetToken and PasswordResetExpiry fields to User entity

            // Generate reset link
            var resetLink = $"https://app.homeservice.com/reset-password?token={resetToken}&email={Uri.EscapeDataString(request.Email)}";

            // Send password reset email
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}",
                resetLink,
                cancellationToken);

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to {Email}", request.Email);
                return Result<bool>.Failure("Failed to send password reset email. Please try again later.");
            }

            _logger.LogInformation("Password reset email sent to {Email}", request.Email);

            return Result<bool>.Success(true, "If the email exists, a password reset link has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for {Email}", request.Email);
            return Result<bool>.Failure("An error occurred while processing your request");
        }
    }

    private string GenerateResetToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private string HashToken(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
