using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetAdminUsersQuery : IRequest<Result<PagedResult<AdminUserListDto>>>
{
    public string? SearchTerm { get; init; }
    public UserRole? Role { get; init; }
    public Region? Region { get; init; }
    public bool? IsVerified { get; init; }
    public DateTime? RegisteredAfter { get; init; }
    public DateTime? RegisteredBefore { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
