using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.ProviderService;

public class DeleteProviderServiceCommand : IRequest<Result<bool>>
{
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }
}
