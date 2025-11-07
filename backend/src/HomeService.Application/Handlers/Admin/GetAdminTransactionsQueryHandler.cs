using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetAdminTransactionsQueryHandler : IRequestHandler<GetAdminTransactionsQuery, Result<PagedResult<AdminTransactionDto>>>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly ILogger<GetAdminTransactionsQueryHandler> _logger;

    public GetAdminTransactionsQueryHandler(
        IRepository<Payment> paymentRepository,
        IRepository<Booking> bookingRepository,
        IRepository<User> userRepository,
        IRepository<Service> serviceRepository,
        ILogger<GetAdminTransactionsQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AdminTransactionDto>>> Handle(GetAdminTransactionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _paymentRepository.GetAllAsync(cancellationToken);
            var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var services = await _serviceRepository.GetAllAsync(cancellationToken);

            // Apply filters
            if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= request.EndDate.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status.Value);
            }

            if (request.PaymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod.Value);
            }

            if (request.Region.HasValue)
            {
                var regionalBookingIds = bookings.Where(b => b.Region == request.Region.Value).Select(b => b.Id).ToList();
                query = query.Where(p => regionalBookingIds.Contains(p.BookingId));
            }

            if (request.CustomerId.HasValue)
            {
                var customerBookingIds = bookings.Where(b => b.CustomerId == request.CustomerId.Value).Select(b => b.Id).ToList();
                query = query.Where(p => customerBookingIds.Contains(p.BookingId));
            }

            if (request.ProviderId.HasValue)
            {
                var providerBookingIds = bookings.Where(b => b.ProviderId == request.ProviderId.Value).Select(b => b.Id).ToList();
                query = query.Where(p => providerBookingIds.Contains(p.BookingId));
            }

            if (request.MinAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= request.MaxAmount.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                var matchingUserIds = users.Where(u =>
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower))
                    .Select(u => u.Id)
                    .ToList();

                var matchingBookingIds = bookings.Where(b =>
                    b.BookingNumber.ToLower().Contains(searchLower) ||
                    matchingUserIds.Contains(b.CustomerId) ||
                    (b.ProviderId.HasValue && matchingUserIds.Contains(b.ProviderId.Value)))
                    .Select(b => b.Id)
                    .ToList();

                query = query.Where(p =>
                    p.TransactionId != null && p.TransactionId.ToLower().Contains(searchLower) ||
                    matchingBookingIds.Contains(p.BookingId));
            }

            // Order by creation date descending
            query = query.OrderByDescending(p => p.CreatedAt);

            var totalCount = query.Count();

            // Apply pagination
            var payments = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var transactionDtos = payments.Select(p =>
            {
                var booking = bookings.FirstOrDefault(b => b.Id == p.BookingId);
                var customer = booking != null ? users.FirstOrDefault(u => u.Id == booking.CustomerId) : null;
                var provider = booking?.ProviderId.HasValue == true ? users.FirstOrDefault(u => u.Id == booking.ProviderId.Value) : null;
                var service = booking != null ? services.FirstOrDefault(s => s.Id == booking.ServiceId) : null;

                var commissionRate = 0.18m;
                var servicePrice = booking?.ServicePrice ?? 0;
                var vatAmount = booking?.VatAmount ?? 0;
                var platformCommission = servicePrice * commissionRate;
                var providerEarnings = servicePrice * (1 - commissionRate);

                return new AdminTransactionDto
                {
                    Id = p.Id,
                    TransactionNumber = $"TXN-{p.Id.ToString().Substring(0, 8).ToUpper()}",
                    CreatedAt = p.CreatedAt,
                    ProcessedAt = p.ProcessedAt,

                    BookingId = p.BookingId,
                    BookingNumber = booking?.BookingNumber ?? "N/A",

                    CustomerId = customer?.Id ?? Guid.Empty,
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                    CustomerEmail = customer?.Email ?? "N/A",

                    ProviderId = provider?.Id,
                    ProviderName = provider != null ? $"{provider.FirstName} {provider.LastName}" : null,
                    ProviderEmail = provider?.Email,

                    ServiceName = service?.Name ?? "Unknown",

                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    TransactionId = p.TransactionId,
                    GatewayResponse = p.GatewayResponse,

                    ServicePrice = servicePrice,
                    VatAmount = vatAmount,
                    PlatformCommission = platformCommission,
                    ProviderEarnings = providerEarnings,

                    Region = booking?.Region ?? Domain.Enums.Region.SaudiArabia
                };
            }).ToList();

            var pagedResult = new PagedResult<AdminTransactionDto>
            {
                Items = transactionDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin transactions");
            return Result.Failure<PagedResult<AdminTransactionDto>>("Error retrieving transactions", ex.Message);
        }
    }
}
