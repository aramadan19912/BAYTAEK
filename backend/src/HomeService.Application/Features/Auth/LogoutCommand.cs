using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<Result<bool>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IRefreshTokenService refreshTokenService,
        ILogger<LogoutCommandHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            _logger.LogInformation("User logged out successfully");

            return Result.Success(true, "Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Result.Failure<bool>("An error occurred during logout", ex.Message);
        }
    }
}
