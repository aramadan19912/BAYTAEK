using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.Content;

public class DeleteContentCommand : IRequest<Result<bool>>
{
    public Guid ContentId { get; set; }
    public Guid AdminUserId { get; set; }
}
