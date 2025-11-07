using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.ProviderVerification;

public class RejectVerificationCommand : IRequest<Result<bool>>
{
    public Guid ProviderId { get; set; }
    public Guid AdminUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
