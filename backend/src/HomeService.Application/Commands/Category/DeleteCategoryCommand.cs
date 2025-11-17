using HomeService.Application.Common;
using MediatR;

namespace HomeService.Application.Commands.Category;

public class DeleteCategoryCommand : IRequest<Result<bool>>
{
    public Guid CategoryId { get; set; }
    public Guid AdminUserId { get; set; }
}
