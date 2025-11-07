using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Favorite;

public class AddFavoriteCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
}
