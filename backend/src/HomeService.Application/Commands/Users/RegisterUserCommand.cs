using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Users;

public record RegisterUserCommand : IRequest<Result<UserDto>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public Language PreferredLanguage { get; init; }
    public Region Region { get; init; }
}
