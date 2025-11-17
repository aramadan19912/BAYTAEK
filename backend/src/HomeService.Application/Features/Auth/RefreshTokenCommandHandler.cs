using HomeService.Application.Interfaces;
using MediatR;

namespace HomeService.Application.Features.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // NOTE: This handler requires IJwtTokenService which violates clean architecture
        // The service is defined in Infrastructure layer and should not be referenced here
        // TODO: Move token generation logic to Application layer or use a facade pattern

        // Get and validate refresh token
        var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Rotate refresh token
        var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(
            refreshToken,
            request.IpAddress,
            request.UserAgent
        );

        // Temporary placeholder - requires proper token generation service
        var newAccessToken = string.Empty;
        var expiryMinutes = 15;

        return new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(expiryMinutes)
        );
    }
}
