using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Provider;

public class ToggleOnlineStatusCommand : IRequest<Result<OnlineStatusDto>>
{
    public Guid ProviderId { get; set; }
    public bool IsOnline { get; set; }
}

public class OnlineStatusDto
{
    public bool IsOnline { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? LastOnlineAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
