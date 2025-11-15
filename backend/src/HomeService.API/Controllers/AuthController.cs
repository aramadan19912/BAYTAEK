using HomeService.Application.Commands.Users;
using HomeService.Application.DTOs.Authentication;
using HomeService.Application.Features.Auth;
using HomeService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IOtpService otpService, ILogger<AuthController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }
    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        // Enrich command with request context
        var enrichedCommand = command with
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        var result = await Mediator.Send(enrichedCommand);

        if (!result.IsSuccess)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Enrich command with request context
        var command = new RefreshTokenCommand(
            request.AccessToken,
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown"
        );

        try
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new RevokeAllTokensCommand(Guid.Parse(userId));
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify email with OTP
    /// </summary>
    [HttpPost("verify-email")]
    [Authorize]
    public async Task<IActionResult> VerifyEmail([FromBody] Application.Handlers.Auth.VerifyEmailCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Verify phone with OTP
    /// </summary>
    [HttpPost("verify-phone")]
    [Authorize]
    public async Task<IActionResult> VerifyPhone([FromBody] Application.Handlers.Auth.VerifyPhoneCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Send OTP code to phone number
    /// </summary>
    [HttpPost("otp/send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _otpService.SendOtpAsync(request.PhoneNumber, request.Purpose, cancellationToken);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        _logger.LogInformation("OTP sent to {PhoneNumber} for purpose {Purpose}", request.PhoneNumber, request.Purpose);
        return Ok(result);
    }

    /// <summary>
    /// Verify OTP code
    /// </summary>
    [HttpPost("otp/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.Code, request.Purpose, cancellationToken);

        if (!result.Success)
            return BadRequest(new { message = result.Message, remainingAttempts = result.RemainingAttempts });

        _logger.LogInformation("OTP verified for {PhoneNumber}", request.PhoneNumber);
        return Ok(result);
    }

    /// <summary>
    /// Resend OTP code
    /// </summary>
    [HttpPost("otp/resend")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtp([FromBody] SendOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _otpService.ResendOtpAsync(request.PhoneNumber, request.Purpose, cancellationToken);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] Application.Handlers.Auth.ForgotPasswordCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] Application.Handlers.Auth.ResetPasswordCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record LogoutRequest(string RefreshToken);
