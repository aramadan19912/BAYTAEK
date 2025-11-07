using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.PromoCode;

public class UpdatePromoCodeStatusCommand : IRequest<Result<bool>>
{
    public Guid PromoCodeId { get; set; }
    public Guid AdminUserId { get; set; }
    public bool IsActive { get; set; }
}
