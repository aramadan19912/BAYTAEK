using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.ProviderVerification;

public class ApproveVerificationCommand : IRequest<Result<bool>>
{
    public Guid ProviderId { get; set; }
    public Guid AdminUserId { get; set; }
    public string? Notes { get; set; }
}
