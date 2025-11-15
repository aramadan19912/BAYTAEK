using MediatR;

namespace HomeService.Application.Features.Auth;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string IpAddress,
    string UserAgent
) : IRequest<RefreshTokenResponse>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
