using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetAdminServicesQueryHandler : IRequestHandler<GetAdminServicesQuery, Result<PagedResult<AdminServiceListDto>>>
{
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly ILogger<GetAdminServicesQueryHandler> _logger;

    public GetAdminServicesQueryHandler(
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<ServiceCategory> categoryRepository,
        ILogger<GetAdminServicesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AdminServiceListDto>>> Handle(GetAdminServicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _serviceRepository.GetAllAsync(cancellationToken);
            var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var reviews = await _reviewRepository.GetAllAsync(cancellationToken);
            var payments = await _paymentRepository.GetAllAsync(cancellationToken);
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            // Apply filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == request.CategoryId.Value);
            }

            if (request.Region.HasValue)
            {
                query = query.Where(s => s.AvailableRegions.Contains(request.Region.Value));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(s => !s.IsDeleted == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.NameEn.ToLower().Contains(searchLower) ||
                    s.NameAr.ToLower().Contains(searchLower) ||
                    s.DescriptionEn.ToLower().Contains(searchLower) ||
                    s.DescriptionAr.ToLower().Contains(searchLower));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice <= request.MaxPrice.Value);
            }

            // Order by creation date descending
            query = query.OrderByDescending(s => s.CreatedAt);

            var totalCount = query.Count();

            // Apply pagination
            var services = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var serviceDtos = services.Select(s =>
            {
                var category = categories.FirstOrDefault(c => c.Id == s.CategoryId);
                var serviceBookings = bookings.Where(b => b.ServiceId == s.Id).ToList();
                var completedBookings = serviceBookings.Where(b => b.Status == Domain.Enums.BookingStatus.Completed).ToList();
                var serviceReviews = reviews.Where(r => serviceBookings.Any(b => b.Id == r.BookingId)).ToList();

                var totalRevenue = payments
                    .Where(p => completedBookings.Any(b => b.Id == p.BookingId) &&
                               p.Status == Domain.Enums.PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                // Get providers for this service (users who have completed at least one booking for this service)
                var providerIds = serviceBookings
                    .Where(b => b.ProviderId.HasValue)
                    .Select(b => b.ProviderId!.Value)
                    .Distinct()
                    .ToList();

                var providers = users.Where(u => providerIds.Contains(u.Id)).ToList();
                var activeProviders = providers.Count(p => !p.IsDeleted && p.LastLoginAt >= DateTime.UtcNow.AddDays(-7));

                return new AdminServiceListDto
                {
                    Id = s.Id,
                    Name = s.NameEn,
                    Description = s.DescriptionEn,
                    BasePrice = s.BasePrice,
                    ImageUrl = s.ImageUrls.FirstOrDefault(),

                    CategoryId = s.CategoryId,
                    CategoryName = category?.NameEn ?? "Unknown",

                    Region = s.AvailableRegions.FirstOrDefault(),
                    IsActive = !s.IsDeleted,

                    TotalBookings = serviceBookings.Count,
                    CompletedBookings = completedBookings.Count,
                    AverageRating = serviceReviews.Any() ? (decimal)serviceReviews.Average(r => r.Rating) : 0,
                    TotalReviews = serviceReviews.Count,
                    TotalRevenue = totalRevenue,

                    TotalProviders = providers.Count,
                    ActiveProviders = activeProviders,

                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt ?? s.CreatedAt
                };
            }).ToList();

            var pagedResult = new PagedResult<AdminServiceListDto>
            {
                Items = serviceDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin services");
            return Result.Failure<PagedResult<AdminServiceListDto>>("Error retrieving services", ex.Message);
        }
    }
}
