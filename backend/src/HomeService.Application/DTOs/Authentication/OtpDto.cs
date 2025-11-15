namespace HomeService.Application.DTOs.Authentication;

public class SendOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "+966"; // Default to Saudi Arabia
    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
}

public class VerifyOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
}

public class OtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public int? RemainingAttempts { get; set; }
}

public enum OtpPurpose
{
    Registration,
    Login,
    PasswordReset,
    PhoneVerification,
    TwoFactorAuthentication
}

public class OtpConfiguration
{
    public int CodeLength { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 10;
    public int MaxAttempts { get; set; } = 3;
    public int ResendCooldownSeconds { get; set; } = 60;
    public bool UseVoiceCall { get; set; } = false;
}
