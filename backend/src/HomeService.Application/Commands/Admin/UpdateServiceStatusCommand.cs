using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Admin;

public record UpdateServiceStatusCommand : IRequest<Result>
{
    public Guid ServiceId { get; init; }
    public bool IsActive { get; init; }
    public Guid AdminUserId { get; init; }
    public string? Reason { get; init; }
}
