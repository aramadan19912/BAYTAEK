using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Address;

public class DeleteAddressCommand : IRequest<Result<bool>>
{
    public Guid AddressId { get; set; }
    public Guid UserId { get; set; }
}
