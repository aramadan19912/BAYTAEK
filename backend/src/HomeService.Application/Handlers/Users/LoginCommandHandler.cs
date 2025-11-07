using AutoMapper;
using HomeService.Application.Commands.Users;
using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Infrastructure.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Users;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
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
            if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
                return Result.Failure<LoginResponse>("Invalid email or password");
            }

            // Generate tokens
            var token = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            var response = new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
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
