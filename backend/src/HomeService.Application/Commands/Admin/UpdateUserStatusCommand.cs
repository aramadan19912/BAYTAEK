using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Admin;

public record UpdateUserStatusCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public bool IsSuspended { get; init; }
    public string? Reason { get; init; }
    public Guid AdminUserId { get; init; }
}
