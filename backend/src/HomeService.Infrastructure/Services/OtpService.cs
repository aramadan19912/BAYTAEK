using HomeService.Application.DTOs.Authentication;
using HomeService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace HomeService.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly ISmsService _smsService;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OtpService> _logger;
    private readonly OtpConfiguration _otpConfig;

    public OtpService(
        ISmsService smsService,
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<OtpService> logger)
    {
        _smsService = smsService;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;

        _otpConfig = new OtpConfiguration
        {
            CodeLength = configuration.GetValue("OTP:CodeLength", 6),
            ExpiryMinutes = configuration.GetValue("OTP:ExpiryMinutes", 10),
            MaxAttempts = configuration.GetValue("OTP:MaxAttempts", 3),
            ResendCooldownSeconds = configuration.GetValue("OTP:ResendCooldownSeconds", 60)
        };
    }

    public async Task<OtpResponse> SendOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        try
        {
            // Normalize phone number
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            // Check cooldown period
            var cooldownKey = GetCooldownKey(normalizedPhone, purpose);
            var cooldownData = await _cache.GetStringAsync(cooldownKey, cancellationToken);
            if (!string.IsNullOrEmpty(cooldownData))
            {
                var cooldownExpiry = DateTime.Parse(cooldownData);
                if (DateTime.UtcNow < cooldownExpiry)
                {
                    var remainingSeconds = (int)(cooldownExpiry - DateTime.UtcNow).TotalSeconds;
                    return new OtpResponse
                    {
                        Success = false,
                        Message = $"Please wait {remainingSeconds} seconds before requesting a new code"
                    };
                }
            }

            // Generate OTP code
            var code = GenerateOtpCode(_otpConfig.CodeLength);

            // Store OTP in cache
            var otpData = new OtpData
            {
                Code = code,
                PhoneNumber = normalizedPhone,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_otpConfig.ExpiryMinutes),
                AttemptsRemaining = _otpConfig.MaxAttempts
            };

            var cacheKey = GetOtpKey(normalizedPhone, purpose);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = otpData.ExpiresAt
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(otpData),
                cacheOptions,
                cancellationToken);

            // Set cooldown
            var cooldownOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddSeconds(_otpConfig.ResendCooldownSeconds)
            };
            await _cache.SetStringAsync(
                cooldownKey,
                DateTime.UtcNow.AddSeconds(_otpConfig.ResendCooldownSeconds).ToString(),
                cooldownOptions,
                cancellationToken);

            // Send SMS
            var message = GetOtpMessage(code, purpose);
            var smsSent = await _smsService.SendSmsAsync(normalizedPhone, message, cancellationToken);

            if (!smsSent)
            {
                _logger.LogError("Failed to send OTP SMS to {PhoneNumber}", normalizedPhone);
                return new OtpResponse
                {
                    Success = false,
                    Message = "Failed to send verification code. Please try again."
                };
            }

            _logger.LogInformation("OTP sent to {PhoneNumber} for purpose {Purpose}", normalizedPhone, purpose);

            return new OtpResponse
            {
                Success = true,
                Message = "Verification code sent successfully",
                ExpiresAt = otpData.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to {PhoneNumber}", phoneNumber);
            return new OtpResponse
            {
                Success = false,
                Message = "An error occurred while sending verification code"
            };
        }
    }

    public async Task<OtpResponse> VerifyOtpAsync(string phoneNumber, string code, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            var cacheKey = GetOtpKey(normalizedPhone, purpose);

            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrEmpty(cachedData))
            {
                return new OtpResponse
                {
                    Success = false,
                    Message = "Verification code has expired or does not exist"
                };
            }

            var otpData = JsonSerializer.Deserialize<OtpData>(cachedData);
            if (otpData == null)
            {
                return new OtpResponse
                {
                    Success = false,
                    Message = "Invalid verification code data"
                };
            }

            // Check expiry
            if (DateTime.UtcNow > otpData.ExpiresAt)
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return new OtpResponse
                {
                    Success = false,
                    Message = "Verification code has expired"
                };
            }

            // Check attempts
            if (otpData.AttemptsRemaining <= 0)
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return new OtpResponse
                {
                    Success = false,
                    Message = "Maximum verification attempts exceeded"
                };
            }

            // Verify code
            if (otpData.Code != code)
            {
                otpData.AttemptsRemaining--;
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(otpData),
                    new DistributedCacheEntryOptions { AbsoluteExpiration = otpData.ExpiresAt },
                    cancellationToken);

                _logger.LogWarning("Invalid OTP attempt for {PhoneNumber}. Remaining attempts: {Attempts}",
                    normalizedPhone, otpData.AttemptsRemaining);

                return new OtpResponse
                {
                    Success = false,
                    Message = "Invalid verification code",
                    RemainingAttempts = otpData.AttemptsRemaining
                };
            }

            // Success - remove OTP from cache
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("OTP verified successfully for {PhoneNumber}", normalizedPhone);

            return new OtpResponse
            {
                Success = true,
                Message = "Verification successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {PhoneNumber}", phoneNumber);
            return new OtpResponse
            {
                Success = false,
                Message = "An error occurred during verification"
            };
        }
    }

    public async Task<OtpResponse> ResendOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        // Resend is same as send with cooldown check
        return await SendOtpAsync(phoneNumber, purpose, cancellationToken);
    }

    public async Task<bool> IsOtpValidAsync(string phoneNumber, string code, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        var result = await VerifyOtpAsync(phoneNumber, code, purpose, cancellationToken);
        return result.Success;
    }

    public async Task InvalidateOtpAsync(string phoneNumber, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);
        var cacheKey = GetOtpKey(normalizedPhone, purpose);
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private string GenerateOtpCode(int length)
    {
        var code = RandomNumberGenerator.GetInt32(
            (int)Math.Pow(10, length - 1),
            (int)Math.Pow(10, length));
        return code.ToString().PadLeft(length, '0');
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters
        return new string(phoneNumber.Where(char.IsDigit).ToArray());
    }

    private string GetOtpKey(string phoneNumber, OtpPurpose purpose)
    {
        return $"otp:{phoneNumber}:{purpose}";
    }

    private string GetCooldownKey(string phoneNumber, OtpPurpose purpose)
    {
        return $"otp:cooldown:{phoneNumber}:{purpose}";
    }

    private string GetOtpMessage(string code, OtpPurpose purpose)
    {
        return purpose switch
        {
            OtpPurpose.Registration => $"Your Home Service registration code is: {code}. Valid for {_otpConfig.ExpiryMinutes} minutes.",
            OtpPurpose.Login => $"Your Home Service login code is: {code}. Valid for {_otpConfig.ExpiryMinutes} minutes.",
            OtpPurpose.PasswordReset => $"Your Home Service password reset code is: {code}. Valid for {_otpConfig.ExpiryMinutes} minutes.",
            OtpPurpose.PhoneVerification => $"Your Home Service verification code is: {code}. Valid for {_otpConfig.ExpiryMinutes} minutes.",
            OtpPurpose.TwoFactorAuthentication => $"Your Home Service 2FA code is: {code}. Valid for {_otpConfig.ExpiryMinutes} minutes.",
            _ => $"Your Home Service verification code is: {code}"
        };
    }

    private class OtpData
    {
        public string Code { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public OtpPurpose Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int AttemptsRemaining { get; set; }
    }
}
