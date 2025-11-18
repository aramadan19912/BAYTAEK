using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.CustomerBooking;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.CustomerBooking;

public class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, Result<CustomerBookingsDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly ILogger<GetCustomerBookingsQueryHandler> _logger;

    public GetCustomerBookingsQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<ServiceCategory> categoryRepository,
        ILogger<GetCustomerBookingsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result<CustomerBookingsDto>> Handle(GetCustomerBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all bookings for customer
            var bookingsQuery = await _bookingRepository.FindAsync(
                b => b.CustomerId == request.CustomerId,
                cancellationToken);

            if (bookingsQuery == null)
            {
                return Result<CustomerBookingsDto>.Success(new CustomerBookingsDto
                {
                    Bookings = new List<CustomerBookingDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = 0
                }, "No bookings found");
            }

            var bookings = bookingsQuery.ToList();

            // Apply filters
            if (request.Status.HasValue)
            {
                bookings = bookings.Where(b => b.Status == request.Status.Value).ToList();
            }

            if (request.StartDate.HasValue)
            {
                bookings = bookings.Where(b => b.ScheduledAt >= request.StartDate.Value).ToList();
            }

            if (request.EndDate.HasValue)
            {
                bookings = bookings.Where(b => b.ScheduledAt <= request.EndDate.Value).ToList();
            }

            // Search term (search in booking number, service name, provider name)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                var searchServiceIds = new HashSet<Guid>();
                var searchProviderIds = new HashSet<Guid>();

                // Search in services
                var services = await _serviceRepository.GetAllAsync(cancellationToken);
                var matchingServices = services.Where(s =>
                    s.NameEn.ToLower().Contains(searchTerm) ||
                    s.NameAr.Contains(searchTerm)).ToList();
                foreach (var service in matchingServices)
                {
                    searchServiceIds.Add(service.Id);
                }

                // Search in providers
                var providers = await _providerRepository.GetAllAsync(cancellationToken);
                var matchingProviders = providers.Where(p =>
                    p.BusinessName != null && p.BusinessName.ToLower().Contains(searchTerm)).ToList();
                foreach (var provider in matchingProviders)
                {
                    searchProviderIds.Add(provider.Id);
                }

                bookings = bookings.Where(b =>
                    b.Id.ToString().ToLower().Contains(searchTerm) ||
                    searchServiceIds.Contains(b.ServiceId) ||
                    (b.ProviderId.HasValue && searchProviderIds.Contains(b.ProviderId.Value))).ToList();
            }

            // Calculate summary statistics
            var allCustomerBookings = await _bookingRepository.FindAsync(
                b => b.CustomerId == request.CustomerId,
                cancellationToken);
            var allBookingsList = allCustomerBookings?.ToList() ?? new List<Domain.Entities.Booking>();

            var totalBookings = allBookingsList.Count;
            var activeBookings = allBookingsList.Count(b =>
                b.Status == BookingStatus.Pending ||
                b.Status == BookingStatus.Confirmed ||
                b.Status == BookingStatus.InProgress);
            var completedBookings = allBookingsList.Count(b => b.Status == BookingStatus.Completed);
            var cancelledBookings = allBookingsList.Count(b => b.Status == BookingStatus.Cancelled);

            // Calculate total spent (from completed bookings)
            var completedBookingIds = allBookingsList
                .Where(b => b.Status == BookingStatus.Completed)
                .Select(b => b.Id)
                .ToList();

            var payments = await _paymentRepository.GetAllAsync(cancellationToken);
            var totalSpent = payments
                .Where(p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            // Order by scheduled date (most recent first)
            bookings = bookings.OrderByDescending(b => b.ScheduledAt).ToList();

            // Pagination
            var totalCount = bookings.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var paginatedBookings = bookings
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Load related entities efficiently
            var serviceIds = paginatedBookings.Select(b => b.ServiceId).Distinct().ToList();
            var providerIds = paginatedBookings.Where(b => b.ProviderId.HasValue).Select(b => b.ProviderId!.Value).Distinct().ToList();
            var addressIds = paginatedBookings.Select(b => b.AddressId).Distinct().ToList();
            var bookingIds = paginatedBookings.Select(b => b.Id).ToList();

            var allServices = await _serviceRepository.GetAllAsync(cancellationToken);
            var servicesDict = allServices
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionary(s => s.Id);

            var allProviders = await _providerRepository.GetAllAsync(cancellationToken);
            var providersDict = allProviders
                .Where(p => providerIds.Contains(p.Id))
                .ToDictionary(p => p.Id);

            var allAddresses = await _addressRepository.GetAllAsync(cancellationToken);
            var addressesDict = allAddresses
                .Where(a => addressIds.Contains(a.Id))
                .ToDictionary(a => a.Id);

            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var paymentsDict = allPayments
                .Where(p => bookingIds.Contains(p.BookingId))
                .GroupBy(p => p.BookingId)
                .ToDictionary(g => g.Key, g => g.First());

            var allReviews = await _reviewRepository.GetAllAsync(cancellationToken);
            var reviewsDict = allReviews
                .Where(r => bookingIds.Contains(r.BookingId))
                .GroupBy(r => r.BookingId)
                .ToDictionary(g => g.Key, g => g.First());

            // Get provider user info
            var providerUserIds = providersDict.Values.Select(p => p.UserId).Distinct().ToList();
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var providerUsersDict = allUsers
                .Where(u => providerUserIds.Contains(u.Id))
                .ToDictionary(u => u.Id);

            // Get categories
            var categoryIds = servicesDict.Values.Select(s => s.CategoryId).Distinct().ToList();
            var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
            var categoriesDict = allCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionary(c => c.Id);

            // Map to DTOs
            var bookingDtos = paginatedBookings.Select(booking =>
            {
                var service = servicesDict.GetValueOrDefault(booking.ServiceId);
                var provider = booking.ProviderId.HasValue ? providersDict.GetValueOrDefault(booking.ProviderId.Value) : null;
                var address = addressesDict.GetValueOrDefault(booking.AddressId);
                var payment = paymentsDict.GetValueOrDefault(booking.Id);
                var hasReview = reviewsDict.ContainsKey(booking.Id);

                var providerUser = provider != null ? providerUsersDict.GetValueOrDefault(provider.UserId) : null;
                var category = service != null ? categoriesDict.GetValueOrDefault(service.CategoryId) : null;

                var providerName = provider?.BusinessName ?? providerUser?.FirstName ?? "Unknown Provider";

                return new CustomerBookingDto
                {
                    BookingId = booking.Id,
                    BookingNumber = $"BK-{booking.Id.ToString().Substring(0, 8).ToUpper()}",
                    Status = booking.Status,
                    // ScheduledAt = booking.ScheduledAt, // Property doesn't exist in CustomerBookingDto
                    CreatedAt = booking.CreatedAt,
                    CompletedAt = booking.CompletedAt,

                    // Service details
                    ServiceId = booking.ServiceId,
                    ServiceNameEn = service?.NameEn ?? "Unknown Service",
                    ServiceNameAr = service?.NameAr ?? "خدمة غير معروفة",
                    ServiceImageUrl = service?.ImageUrls?.FirstOrDefault(),
                    CategoryNameEn = category?.NameEn ?? "Unknown Category",
                    CategoryNameAr = category?.NameAr ?? "فئة غير معروفة",

                    // Provider details
                    ProviderId = booking.ProviderId ?? Guid.Empty,
                    ProviderName = providerName,
                    ProviderProfileImageUrl = providerUser?.ProfileImageUrl,
                    ProviderRating = provider?.AverageRating ?? 0,
                    IsProviderVerified = provider?.IsVerified ?? false,

                    // Address details
                    AddressId = booking.AddressId,
                    AddressLabel = address?.Label ?? "Unknown Address",
                    FullAddress = address?.FullAddress ?? "",

                    // Pricing
                    ServicePrice = service?.BasePrice ?? booking.TotalAmount,
                    TotalAmount = booking.TotalAmount,
                    Currency = booking.Currency.ToString(),

                    // Payment
                    IsPaid = payment != null && payment.Status == PaymentStatus.Completed,
                    PaymentStatus = payment?.Status,

                    // Review
                    HasReviewed = hasReview,

                    // Additional info
                    CustomerNotes = booking.SpecialInstructions,
                    ProviderNotes = booking.ProviderNotes
                };
            }).ToList();

            var result = new CustomerBookingsDto
            {
                Bookings = bookingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,

                // Summary statistics
                TotalBookings = totalBookings,
                ActiveBookings = activeBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                TotalSpent = totalSpent
            };

            return Result<CustomerBookingsDto>.Success(result, "Customer bookings retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer bookings for customer {CustomerId}", request.CustomerId);
            return Result<CustomerBookingsDto>.Failure("An error occurred while retrieving bookings");
        }
    }
}
