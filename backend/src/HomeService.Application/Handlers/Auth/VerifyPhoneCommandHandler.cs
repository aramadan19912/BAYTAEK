using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Auth;

public record VerifyPhoneCommand(string PhoneNumber, string OtpCode) : IRequest<Result<bool>>;

public class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IOtpService _otpService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyPhoneCommandHandler> _logger;

    public VerifyPhoneCommandHandler(
        IRepository<Domain.Entities.User> userRepository,
        IOtpService otpService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyPhoneCommandHandler> logger)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by phone number
            var users = await _userRepository.FindAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
            var user = users?.FirstOrDefault();

            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            if (user.PhoneVerified)
            {
                return Result<bool>.Success(true, "Phone number already verified");
            }

            // Verify OTP
            var otpResult = await _otpService.VerifyOtpAsync(
                request.PhoneNumber,
                request.OtpCode,
                Application.DTOs.Authentication.OtpPurpose.PhoneVerification,
                cancellationToken);

            if (!otpResult.Success)
            {
                return Result<bool>.Failure(otpResult.Message ?? "Invalid OTP code");
            }

            // Mark phone as verified
            user.IsPhoneVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Phone verified for user {PhoneNumber}", request.PhoneNumber);

            return Result<bool>.Success(true, "Phone number verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying phone for {PhoneNumber}", request.PhoneNumber);
            return Result<bool>.Failure("An error occurred during phone verification");
        }
    }
}
