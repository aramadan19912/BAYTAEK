using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetAdminBookingsQueryHandler : IRequestHandler<GetAdminBookingsQuery, Result<PagedResult<AdminBookingListDto>>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly ILogger<GetAdminBookingsQueryHandler> _logger;

    public GetAdminBookingsQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<ServiceCategory> categoryRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        ILogger<GetAdminBookingsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _categoryRepository = categoryRepository;
        _addressRepository = addressRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AdminBookingListDto>>> Handle(GetAdminBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _bookingRepository.GetAllAsync(cancellationToken);
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var services = await _serviceRepository.GetAllAsync(cancellationToken);
            var payments = await _paymentRepository.GetAllAsync(cancellationToken);
            var reviews = await _reviewRepository.GetAllAsync(cancellationToken);
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var addresses = await _addressRepository.GetAllAsync(cancellationToken);

            // Apply filters
            if (request.Status.HasValue)
            {
                query = query.Where(b => b.Status == request.Status.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(b => b.ScheduledDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(b => b.ScheduledDate <= request.EndDate.Value);
            }

            if (request.Region.HasValue)
            {
                query = query.Where(b => b.Region == request.Region.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == request.CustomerId.Value);
            }

            if (request.ProviderId.HasValue)
            {
                query = query.Where(b => b.ProviderId == request.ProviderId.Value);
            }

            if (request.ServiceId.HasValue)
            {
                query = query.Where(b => b.ServiceId == request.ServiceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                var matchingUserIds = users.Where(u =>
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.PhoneNumber.Contains(searchLower))
                    .Select(u => u.Id)
                    .ToList();

                query = query.Where(b =>
                    matchingUserIds.Contains(b.CustomerId) ||
                    (b.ProviderId.HasValue && matchingUserIds.Contains(b.ProviderId.Value)) ||
                    b.Id.ToString().Contains(searchLower));
            }

            if (request.IsPaid.HasValue)
            {
                var paidBookingIds = payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed)
                    .Select(p => p.BookingId)
                    .ToList();

                query = request.IsPaid.Value
                    ? query.Where(b => paidBookingIds.Contains(b.Id))
                    : query.Where(b => !paidBookingIds.Contains(b.Id));
            }

            // Order by creation date descending
            query = query.OrderByDescending(b => b.CreatedAt);

            var totalCount = query.Count();

            // Apply pagination
            var bookings = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var bookingDtos = bookings.Select(b =>
            {
                var customer = users.FirstOrDefault(u => u.Id == b.CustomerId);
                var provider = b.ProviderId.HasValue ? users.FirstOrDefault(u => u.Id == b.ProviderId.Value) : null;
                var service = services.FirstOrDefault(s => s.Id == b.ServiceId);
                var category = service != null ? categories.FirstOrDefault(c => c.Id == service.CategoryId) : null;
                var payment = payments.FirstOrDefault(p => p.BookingId == b.Id);
                var review = reviews.FirstOrDefault(r => r.BookingId == b.Id);
                var address = addresses.FirstOrDefault(a => a.Id == b.AddressId);

                var servicePrice = service?.BasePrice ?? 0;
                var totalPrice = b.TotalAmount;

                return new AdminBookingListDto
                {
                    Id = b.Id,
                    BookingNumber = $"BK-{b.Id.ToString().Substring(0, 8).ToUpper()}",
                    CreatedAt = b.CreatedAt,
                    ScheduledDate = b.ScheduledAt,
                    Status = b.Status,

                    CustomerId = b.CustomerId,
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                    CustomerEmail = customer?.Email ?? "N/A",
                    CustomerPhone = customer?.PhoneNumber ?? "N/A",

                    ProviderId = b.ProviderId,
                    ProviderName = provider != null ? $"{provider.FirstName} {provider.LastName}" : null,
                    ProviderEmail = provider?.Email,
                    ProviderPhone = provider?.PhoneNumber,

                    ServiceId = b.ServiceId,
                    ServiceName = service?.NameEn ?? "Unknown",
                    CategoryName = category?.NameEn ?? "Unknown",

                    Region = b.Region,
                    Address = address?.FullAddress ?? "N/A",
                    ServicePrice = servicePrice,
                    VatAmount = b.VatAmount,
                    TotalPrice = totalPrice,
                    PlatformCommission = servicePrice * 0.18m, // 18% commission

                    IsPaid = payment?.Status == Domain.Enums.PaymentStatus.Completed,
                    PaymentMethod = payment?.PaymentMethod.ToString(),
                    CompletedAt = b.CompletedAt,
                    CancelledAt = b.CancelledAt,
                    CancellationReason = b.CancellationReason,

                    HasReview = review != null,
                    ReviewRating = review?.Rating
                };
            }).ToList();

            var pagedResult = new PagedResult<AdminBookingListDto>
            {
                Items = bookingDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin bookings");
            return Result.Failure<PagedResult<AdminBookingListDto>>("Error retrieving bookings", ex.Message);
        }
    }
}
