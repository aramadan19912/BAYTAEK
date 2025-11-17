using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.User;

public class UpdateUserProfileCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? ProfileImageUrl { get; set; }
}
