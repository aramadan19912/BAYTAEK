using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Auth;

public record RevokeAllTokensCommand(Guid UserId) : IRequest<Result<bool>>;

public class RevokeAllTokensCommandHandler : IRequestHandler<RevokeAllTokensCommand, Result<bool>>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<RevokeAllTokensCommandHandler> _logger;

    public RevokeAllTokensCommandHandler(
        IRefreshTokenService refreshTokenService,
        ILogger<RevokeAllTokensCommandHandler> logger)
    {
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RevokeAllTokensCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _refreshTokenService.RevokeAllUserTokensAsync(request.UserId);

            _logger.LogInformation("All tokens revoked for user: {UserId}", request.UserId);

            return Result.Success(true, "All tokens revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", request.UserId);
            return Result.Failure<bool>("An error occurred while revoking tokens", ex.Message);
        }
    }
}
