using HomeService.Application.DTOs.Authentication;

namespace HomeService.Application.Interfaces;

public interface IOtpService
{
    /// <summary>
    /// Generate and send OTP code to phone number
    /// </summary>
    Task<OtpResponse> SendOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify OTP code
    /// </summary>
    Task<OtpResponse> VerifyOtpAsync(string phoneNumber, string code, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend OTP code
    /// </summary>
    Task<OtpResponse> ResendOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if OTP is valid and not expired
    /// </summary>
    Task<bool> IsOtpValidAsync(string phoneNumber, string code, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate OTP after successful verification or max attempts
    /// </summary>
    Task InvalidateOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default);
}

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<bool> SendSmsWithTemplateAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
