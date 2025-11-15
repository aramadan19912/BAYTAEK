using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.Admin;

public record GetAdminTransactionsQuery : IRequest<Result<PagedResult<AdminTransactionDto>>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public PaymentStatus? Status { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public Region? Region { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ProviderId { get; init; }
    public string? SearchTerm { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
