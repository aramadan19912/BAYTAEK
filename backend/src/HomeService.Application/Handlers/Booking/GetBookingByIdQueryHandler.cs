using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Booking;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Booking;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDetailDto>>
{
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly ILogger<GetBookingByIdQueryHandler> _logger;

    public GetBookingByIdQueryHandler(
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        ILogger<GetBookingByIdQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _serviceRepository = serviceRepository;
        _addressRepository = addressRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<BookingDetailDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<BookingDetailDto>.Failure("Booking not found");
            }

            // Authorization check - user must be customer or provider of this booking
            if (booking.CustomerId != request.UserId && booking.ProviderId != request.UserId)
            {
                _logger.LogWarning("Unauthorized access attempt to booking {BookingId} by user {UserId}",
                    request.BookingId, request.UserId);
                return Result<BookingDetailDto>.Failure("You are not authorized to view this booking");
            }

            // Get related entities
            var customer = await _userRepository.GetByIdAsync(booking.CustomerId, cancellationToken);
            var providerUser = booking.ProviderId.HasValue ? await _userRepository.GetByIdAsync(booking.ProviderId.Value, cancellationToken) : null;
            var provider = booking.ProviderId.HasValue ? await _providerRepository.FindAsync(
                p => p.UserId == booking.ProviderId.Value,
                cancellationToken) : null;
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
            var address = await _addressRepository.GetByIdAsync(booking.AddressId, cancellationToken);

            // Get payment if exists
            var payment = await _paymentRepository.FindAsync(
                p => p.BookingId == booking.Id,
                cancellationToken);

            // Get review if exists
            var review = await _reviewRepository.FindAsync(
                r => r.BookingId == booking.Id,
                cancellationToken);

            // Build DTO
            var dto = new BookingDetailDto
            {
                Id = booking.Id,
                Status = booking.Status.ToString(),

                // Service Info
                ServiceId = booking.ServiceId,
                ServiceNameEn = service?.NameEn ?? "Unknown Service",
                ServiceNameAr = service?.NameAr ?? "خدمة غير معروفة",
                ServiceDescriptionEn = service?.DescriptionEn ?? string.Empty,
                ServiceDescriptionAr = service?.DescriptionAr ?? string.Empty,
                ServiceImages = service?.ImageUrls?.ToList() ?? new List<string>(),

                // Customer Info
                CustomerId = booking.CustomerId,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer",
                CustomerEmail = customer?.Email ?? string.Empty,
                CustomerPhone = customer?.PhoneNumber ?? string.Empty,
                CustomerProfileImage = customer?.ProfileImageUrl,

                // Provider Info
                ProviderId = booking.ProviderId ?? Guid.Empty,
                ProviderName = providerUser != null ? $"{providerUser.FirstName} {providerUser.LastName}" : "Unknown Provider",
                ProviderBusinessName = provider?.FirstOrDefault()?.BusinessName,
                ProviderPhone = providerUser?.PhoneNumber ?? string.Empty,
                ProviderProfileImage = providerUser?.ProfileImageUrl,
                ProviderAverageRating = provider?.FirstOrDefault()?.AverageRating ?? 0,
                ProviderTotalReviews = provider?.FirstOrDefault()?.TotalReviews ?? 0,

                // Location Info
                AddressId = booking.AddressId,
                AddressLabel = address?.Label ?? string.Empty,
                Street = address?.Street ?? string.Empty,
                City = address?.City ?? string.Empty,
                Region = address != null ? address.Region.ToString() : string.Empty,
                Latitude = address?.Latitude,
                Longitude = address?.Longitude,
                BuildingNumber = address?.BuildingNumber,
                Floor = address?.Floor,
                ApartmentNumber = address?.ApartmentNumber,
                AdditionalDirections = address?.AdditionalDirections,

                // Booking Details
                ScheduledAt = booking.ScheduledAt,
                StartedAt = booking.StartedAt,
                CompletedAt = booking.CompletedAt,
                CancelledAt = booking.CancelledAt,
                SpecialInstructions = booking.SpecialInstructions,

                // Pricing
                TotalAmount = booking.TotalAmount,
                VatAmount = booking.VatAmount,
                VatPercentage = booking.VatPercentage,
                Currency = booking.Currency.ToString(),

                // Payment
                IsPaid = payment?.FirstOrDefault()?.Status == Domain.Enums.PaymentStatus.Completed,
                PaymentId = payment?.FirstOrDefault()?.Id,
                PaymentMethod = payment?.FirstOrDefault()?.PaymentMethod.ToString(),
                PaymentStatus = payment?.FirstOrDefault()?.Status.ToString(),
                TransactionId = payment?.FirstOrDefault()?.TransactionId,

                // Review
                ReviewId = review?.FirstOrDefault()?.Id,
                Rating = review?.FirstOrDefault()?.Rating,
                ReviewComment = review?.FirstOrDefault()?.Comment,

                // Timestamps
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,

                // Recurring
                IsRecurring = booking.IsRecurring,
                RecurrencePattern = booking.RecurrencePattern
            };

            _logger.LogInformation("Retrieved booking details for {BookingId} by user {UserId}",
                request.BookingId, request.UserId);

            return Result<BookingDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking {BookingId}", request.BookingId);
            return Result<BookingDetailDto>.Failure("An error occurred while retrieving the booking");
        }
    }
}
