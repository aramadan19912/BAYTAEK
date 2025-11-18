using AutoMapper;
using HomeService.Application.Commands.Users;
using HomeService.Application.Common;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Users;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRefreshTokenService refreshTokenService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenService = refreshTokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var users = await _userRepository.FindAsync(
                u => u.Email == request.Email,
                cancellationToken);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                return Result.Failure<LoginResponse>("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
                return Result.Failure<LoginResponse>("Invalid email or password");
            }

            // Generate access token  
            // TODO: Implement JWT token generation
            var token = "temp-token-" + Guid.NewGuid().ToString();

            // Generate and persist refresh token
            var refreshTokenEntity = await _refreshTokenService.GenerateRefreshTokenAsync(
                user.Id,
                request.IpAddress ?? "Unknown",
                request.UserAgent ?? "Unknown",
                request.DeviceId
            );

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshTokenEntity.Token,
                User = _mapper.Map<UserDto>(user)
            };

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            return Result.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return Result.Failure<LoginResponse>("An error occurred during login", ex.Message);
        }
    }
}
