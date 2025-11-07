using HomeService.Application.Commands.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

public class AuthController : BaseApiController
{
    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify email with OTP
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        // To be implemented
        return Ok(new { message = "Email verification endpoint" });
    }

    /// <summary>
    /// Verify phone with OTP
    /// </summary>
    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneCommand command)
    {
        // To be implemented
        return Ok(new { message = "Phone verification endpoint" });
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        // To be implemented
        return Ok(new { message = "Password reset email sent" });
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        // To be implemented
        return Ok(new { message = "Password reset successful" });
    }
}

// Placeholder commands for future implementation
public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<Result<LoginResponse>>;
public record VerifyEmailCommand(string Email, string OtpCode) : IRequest<Result>;
public record VerifyPhoneCommand(string PhoneNumber, string OtpCode) : IRequest<Result>;
public record ForgotPasswordCommand(string Email) : IRequest<Result>;
public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result>;
