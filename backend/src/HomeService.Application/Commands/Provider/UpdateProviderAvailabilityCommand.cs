using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Provider;

public class UpdateProviderAvailabilityCommand : IRequest<Result<bool>>
{
    public Guid ProviderId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
}
