using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Auth;

public record VerifyEmailCommand(string Email, string OtpCode) : IRequest<Result<bool>>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IOtpService _otpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IRepository<Domain.Entities.User> userRepository,
        IOtpService _otpService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        this._otpService = _otpService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var users = await _userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            var user = users?.FirstOrDefault();

            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            if (user.EmailVerified)
            {
                return Result<bool>.Success(true, "Email already verified");
            }

            // Verify OTP using phone number (assuming OTP was sent to phone for email verification)
            var otpResult = await _otpService.VerifyOtpAsync(
                user.PhoneNumber,
                request.OtpCode,
                Application.DTOs.Authentication.OtpPurpose.Registration,
                cancellationToken);

            if (!otpResult.Success)
            {
                return Result<bool>.Failure(otpResult.Message ?? "Invalid OTP code");
            }

            // Mark email as verified
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified for user {Email}", request.Email);

            return Result<bool>.Success(true, "Email verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for {Email}", request.Email);
            return Result<bool>.Failure("An error occurred during email verification");
        }
    }
}
