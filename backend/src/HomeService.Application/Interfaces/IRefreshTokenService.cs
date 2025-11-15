using HomeService.Domain.Entities;

namespace HomeService.Application.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Generate a new refresh token for a user
    /// </summary>
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress, string userAgent, string? deviceId = null);

    /// <summary>
    /// Validate and get refresh token
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>
    /// Revoke a refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null);

    /// <summary>
    /// Revoke all refresh tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId);

    /// <summary>
    /// Remove expired tokens
    /// </summary>
    Task RemoveExpiredTokensAsync();

    /// <summary>
    /// Rotate refresh token (revoke old, create new)
    /// </summary>
    Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress, string userAgent);
}
