using AutoMapper;
using HomeService.Application.Commands.Users;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Users;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<HomeService.Application.DTOs.UserDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<HomeService.Application.DTOs.UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUsers = await _userRepository.FindAsync(
                u => u.Email == request.Email || u.PhoneNumber == request.PhoneNumber,
                cancellationToken);

            if (existingUsers.Any())
            {
                return Result.Failure<HomeService.Application.DTOs.UserDto>("User with this email or phone number already exists");
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = request.Role,
                PreferredLanguage = request.PreferredLanguage,
                Region = request.Region,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsTwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<HomeService.Application.DTOs.UserDto>(user);

            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            return Result.Success(userDto, "User registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Email}", request.Email);
            return Result.Failure<HomeService.Application.DTOs.UserDto>("An error occurred while registering user", ex.Message);
        }
    }
}
