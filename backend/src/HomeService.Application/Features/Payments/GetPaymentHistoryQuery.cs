using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Payments;

public record GetPaymentHistoryQuery(
    Guid UserId,
    PaymentStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<PaymentHistoryDto>>>;

public class GetPaymentHistoryQueryHandler
    : IRequestHandler<GetPaymentHistoryQuery, Result<PagedResult<PaymentHistoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPaymentHistoryQueryHandler> _logger;

    public GetPaymentHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPaymentHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PaymentHistoryDto>>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking.Provider)
                    .ThenInclude(sp => sp.User)
                .Where(p => p.Booking.CustomerId == request.UserId);

            // Apply filters
            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status.Value);

            if (request.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(p => p.CreatedAt <= request.EndDate.Value);

            // Order by date (most recent first)
            query = query.OrderByDescending(p => p.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var payments = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var paymentDtos = payments.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                Currency = p.Currency.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                ProcessedAt = p.ProcessedAt,
                RefundAmount = p.RefundAmount,
                RefundedAt = p.RefundedAt,
                ServiceName = p.Booking.Service.NameEn,
                ProviderName = p.Booking.Provider != null
                    ? $"{p.Booking.Provider.User.FirstName} {p.Booking.Provider.User.LastName}"
                    : null,
                CreatedAt = p.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<PaymentHistoryDto>
            {
                Items = paymentDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", request.UserId);
            return Result.Failure<PagedResult<PaymentHistoryDto>>(
                "An error occurred while retrieving payment history",
                ex.Message);
        }
    }
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public DateTime CreatedAt { get; set; }
}
