using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.CustomerBooking;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.CustomerBooking;

public class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, Result<CustomerBookingsDto>>
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Review> _reviewRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ILogger<GetCustomerBookingsQueryHandler> _logger;

    public GetCustomerBookingsQueryHandler(
        IRepository<Booking> bookingRepository,
        IRepository<Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<User> userRepository,
        IRepository<Address> addressRepository,
        IRepository<Payment> paymentRepository,
        IRepository<Review> reviewRepository,
        IRepository<Category> categoryRepository,
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
                bookings = bookings.Where(b => b.ScheduledDateTime >= request.StartDate.Value).ToList();
            }

            if (request.EndDate.HasValue)
            {
                bookings = bookings.Where(b => b.ScheduledDateTime <= request.EndDate.Value).ToList();
            }

            // Search term (search in booking number, service name, provider name)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                var serviceIds = new HashSet<Guid>();
                var providerIds = new HashSet<Guid>();

                // Search in services
                var services = await _serviceRepository.GetAllAsync(cancellationToken);
                var matchingServices = services.Where(s =>
                    s.NameEn.ToLower().Contains(searchTerm) ||
                    s.NameAr.Contains(searchTerm)).ToList();
                foreach (var service in matchingServices)
                {
                    serviceIds.Add(service.Id);
                }

                // Search in providers
                var providers = await _providerRepository.GetAllAsync(cancellationToken);
                var matchingProviders = providers.Where(p =>
                    p.BusinessName != null && p.BusinessName.ToLower().Contains(searchTerm)).ToList();
                foreach (var provider in matchingProviders)
                {
                    providerIds.Add(provider.Id);
                }

                bookings = bookings.Where(b =>
                    b.BookingNumber.ToLower().Contains(searchTerm) ||
                    serviceIds.Contains(b.ServiceId) ||
                    providerIds.Contains(b.ProviderId)).ToList();
            }

            // Calculate summary statistics
            var allCustomerBookings = await _bookingRepository.FindAsync(
                b => b.CustomerId == request.CustomerId,
                cancellationToken);
            var allBookingsList = allCustomerBookings?.ToList() ?? new List<Booking>();

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
            bookings = bookings.OrderByDescending(b => b.ScheduledDateTime).ToList();

            // Pagination
            var totalCount = bookings.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var paginatedBookings = bookings
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Load related entities efficiently
            var serviceIds = paginatedBookings.Select(b => b.ServiceId).Distinct().ToList();
            var providerIds = paginatedBookings.Select(b => b.ProviderId).Distinct().ToList();
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
                var provider = providersDict.GetValueOrDefault(booking.ProviderId);
                var address = addressesDict.GetValueOrDefault(booking.AddressId);
                var payment = paymentsDict.GetValueOrDefault(booking.Id);
                var hasReview = reviewsDict.ContainsKey(booking.Id);

                var providerUser = provider != null ? providerUsersDict.GetValueOrDefault(provider.UserId) : null;
                var category = service != null ? categoriesDict.GetValueOrDefault(service.CategoryId) : null;

                var providerName = provider?.BusinessName ?? providerUser?.FirstName ?? "Unknown Provider";

                return new CustomerBookingDto
                {
                    BookingId = booking.Id,
                    BookingNumber = booking.BookingNumber,
                    Status = booking.Status,
                    ScheduledDateTime = booking.ScheduledDateTime,
                    CreatedAt = booking.CreatedAt,
                    CompletedAt = booking.CompletedAt,

                    // Service details
                    ServiceId = booking.ServiceId,
                    ServiceNameEn = service?.NameEn ?? "Unknown Service",
                    ServiceNameAr = service?.NameAr ?? "خدمة غير معروفة",
                    ServiceImageUrl = service?.Images?.FirstOrDefault(),
                    CategoryNameEn = category?.NameEn ?? "Unknown Category",
                    CategoryNameAr = category?.NameAr ?? "فئة غير معروفة",

                    // Provider details
                    ProviderId = booking.ProviderId,
                    ProviderName = providerName,
                    ProviderProfileImageUrl = providerUser?.ProfileImageUrl,
                    ProviderRating = provider?.AverageRating ?? 0,
                    IsProviderVerified = provider?.IsVerified ?? false,

                    // Address details
                    AddressId = booking.AddressId,
                    AddressLabel = address?.Label ?? "Unknown Address",
                    FullAddress = address?.FullAddress ?? "",

                    // Pricing
                    ServicePrice = booking.ServicePrice,
                    TotalAmount = booking.TotalAmount,
                    Currency = booking.Currency,

                    // Payment
                    IsPaid = booking.IsPaid,
                    PaymentStatus = payment?.Status,

                    // Review
                    HasReviewed = hasReview,

                    // Additional info
                    CustomerNotes = booking.CustomerNotes,
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
