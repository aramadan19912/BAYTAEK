using HomeService.Application.Interfaces;
using HomeService.Infrastructure.Identity;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace HomeService.Application.Features.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate the access token (can be expired)
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Invalid access token");
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        var userId = Guid.Parse(userIdClaim.Value);

        // Get and validate refresh token
        var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive || refreshToken.UserId != userId)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Rotate refresh token
        var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(
            refreshToken,
            request.IpAddress,
            request.UserAgent
        );

        // Generate new access token
        var newAccessToken = _jwtTokenService.GenerateToken(refreshToken.User);

        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(expiryMinutes)
        );
    }
}
