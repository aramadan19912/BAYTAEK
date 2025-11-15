using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace HomeService.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress, string userAgent, string? deviceId = null)
    {
        var refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceId = deviceId
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();

        var refreshToken = await refreshTokenRepo
            .GetQueryable()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null)
    {
        var refreshToken = await GetRefreshTokenAsync(token);

        if (refreshToken == null || !refreshToken.IsActive)
            return;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = replacedByToken;

        _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();

        var activeTokens = await refreshTokenRepo
            .GetQueryable()
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            refreshTokenRepo.Update(token);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveExpiredTokensAsync()
    {
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();

        var expiredTokens = await refreshTokenRepo
            .GetQueryable()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
            .Where(rt => rt.CreatedAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days for audit
            .ToListAsync();

        foreach (var token in expiredTokens)
        {
            refreshTokenRepo.Delete(token);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress, string userAgent)
    {
        var newRefreshToken = await GenerateRefreshTokenAsync(oldToken.UserId, ipAddress, userAgent, oldToken.DeviceId);
        await RevokeRefreshTokenAsync(oldToken.Token, newRefreshToken.Token);

        return newRefreshToken;
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
