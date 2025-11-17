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

public class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, Result<CustomerBookingDetailDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<HomeService.Domain.Entities.User> _userRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly IRepository<HomeService.Domain.Entities.Payment> _paymentRepository;
    private readonly IRepository<HomeService.Domain.Entities.Review> _reviewRepository;
    private readonly ILogger<GetBookingDetailQueryHandler> _logger;

    public GetBookingDetailQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        ILogger<GetBookingDetailQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<Result<CustomerBookingDetailDto>> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return Result<CustomerBookingDetailDto>.Failure("Booking not found");
            }

            // Verify customer owns this booking
            if (booking.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Customer {CustomerId} attempted to access booking {BookingId} they don't own",
                    request.CustomerId, request.BookingId);
                return Result<CustomerBookingDetailDto>.Failure("You are not authorized to view this booking");
            }

            // Get related entities
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId, cancellationToken);
            var provider = await _providerRepository.GetByIdAsync(booking.ProviderId, cancellationToken);
            var address = await _addressRepository.GetByIdAsync(booking.AddressId, cancellationToken);

            User? providerUser = null;
            if (provider != null)
            {
                providerUser = await _userRepository.GetByIdAsync(provider.UserId, cancellationToken);
            }

            Category? category = null;
            if (service != null)
            {
            }

            // Get payment
            var payments = await _paymentRepository.FindAsync(
                p => p.BookingId == booking.Id,
                cancellationToken);
            var payment = payments?.FirstOrDefault();

            // Get review
            var reviews = await _reviewRepository.FindAsync(
                r => r.BookingId == booking.Id,
                cancellationToken);
            var review = reviews?.FirstOrDefault();

            // Build timeline
            var timeline = new List<BookingTimelineDto>();

            timeline.Add(new BookingTimelineDto
            {
                Status = BookingStatus.Pending,
                Timestamp = booking.CreatedAt,
                Notes = "Booking created"
            });

            if (booking.ConfirmedAt.HasValue)
            {
                timeline.Add(new BookingTimelineDto
                {
                    Status = BookingStatus.Confirmed,
                    Timestamp = booking.ConfirmedAt.Value,
                    Notes = "Booking confirmed by provider"
                });
            }

            if (booking.StartedAt.HasValue)
            {
                timeline.Add(new BookingTimelineDto
                {
                    Status = BookingStatus.InProgress,
                    Timestamp = booking.StartedAt.Value,
                    Notes = "Service started"
                });
            }

            if (booking.CompletedAt.HasValue)
            {
                timeline.Add(new BookingTimelineDto
                {
                    Status = BookingStatus.Completed,
                    Timestamp = booking.CompletedAt.Value,
                    Notes = "Service completed"
                });
            }

            if (booking.CancelledAt.HasValue)
            {
                timeline.Add(new BookingTimelineDto
                {
                    Status = BookingStatus.Cancelled,
                    Timestamp = booking.CancelledAt.Value,
                    Notes = !string.IsNullOrEmpty(booking.CancellationReason)
                        ? $"Cancelled: {booking.CancellationReason}"
                        : "Booking cancelled"
                });
            }

            // Sort timeline by timestamp
            timeline = timeline.OrderBy(t => t.Timestamp).ToList();

            // Map to DTO
            var result = new CustomerBookingDetailDto
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                Status = booking.Status,
                ScheduledDateTime = booking.ScheduledDateTime,
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.ConfirmedAt,
                StartedAt = booking.StartedAt,
                CompletedAt = booking.CompletedAt,
                CancelledAt = booking.CancelledAt,

                // Service details
                Service = new ServiceInfoDto
                {
                    ServiceId = service?.Id ?? Guid.Empty,
                    NameEn = service?.NameEn ?? "Unknown Service",
                    NameAr = service?.NameAr ?? "خدمة غير معروفة",
                    DescriptionEn = service?.DescriptionEn,
                    DescriptionAr = service?.DescriptionAr,
                    ImageUrl = service?.Images?.FirstOrDefault(),
                    CategoryNameEn = category?.NameEn ?? "Unknown Category",
                    CategoryNameAr = category?.NameAr ?? "فئة غير معروفة",
                    BasePrice = service?.BasePrice ?? 0,
                    EstimatedDurationMinutes = service?.EstimatedDurationMinutes ?? 0
                },

                // Provider details
                Provider = new ProviderInfoDto
                {
                    ProviderId = provider?.Id ?? Guid.Empty,
                    Name = providerUser?.FirstName ?? "Unknown Provider",
                    BusinessName = provider?.BusinessName,
                    ProfileImageUrl = providerUser?.ProfileImageUrl,
                    PhoneNumber = providerUser?.PhoneNumber,
                    Email = providerUser?.Email,
                    Rating = provider?.AverageRating ?? 0,
                    TotalReviews = provider?.TotalReviews ?? 0,
                    IsVerified = provider?.IsVerified ?? false,
                    YearsOfExperience = provider?.YearsOfExperience ?? 0
                },

                // Address details
                Address = new AddressInfoDto
                {
                    AddressId = address?.Id ?? Guid.Empty,
                    Label = address?.Label ?? "Unknown Address",
                    FullAddress = address?.FullAddress ?? "",
                    BuildingNumber = address?.BuildingNumber,
                    Street = address?.Street,
                    District = address?.District,
                    City = address?.City ?? "",
                    Region = address?.Region.ToString() ?? "",
                    PostalCode = address?.PostalCode,
                    Latitude = address?.Latitude ?? 0,
                    Longitude = address?.Longitude ?? 0,
                    AdditionalInfo = address?.AdditionalInfo
                },

                // Pricing breakdown
                Pricing = new PricingInfoDto
                {
                    ServicePrice = booking.ServicePrice,
                    TaxAmount = booking.TaxAmount,
                    DiscountAmount = booking.DiscountAmount,
                    TotalAmount = booking.TotalAmount,
                    Currency = booking.Currency
                },

                // Payment details
                Payment = payment != null ? new PaymentInfoDto
                {
                    PaymentId = payment.Id,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    Amount = payment.Amount,
                    PaidAt = payment.CreatedAt,
                    TransactionId = payment.TransactionId
                } : null,

                // Review details
                Review = review != null ? new ReviewInfoDto
                {
                    ReviewId = review.Id,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    Images = review.Images?.ToList() ?? new List<string>(),
                    CreatedAt = review.CreatedAt,
                    ProviderResponse = review.ProviderResponse,
                    ProviderRespondedAt = review.ProviderRespondedAt
                } : null,

                // Timeline
                Timeline = timeline,

                // Notes
                CustomerNotes = booking.CustomerNotes,
                ProviderNotes = booking.ProviderNotes,
                AdminNotes = booking.AdminNotes,

                // Photos
                BeforePhotos = booking.BeforePhotos?.ToList() ?? new List<string>(),
                AfterPhotos = booking.AfterPhotos?.ToList() ?? new List<string>(),

                // Cancellation info
                CancellationReason = booking.CancellationReason,
                RefundAmount = booking.RefundAmount,
                RefundStatus = booking.RefundStatus
            };

            return Result<CustomerBookingDetailDto>.Success(result, "Booking details retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking details for booking {BookingId}", request.BookingId);
            return Result<CustomerBookingDetailDto>.Failure("An error occurred while retrieving booking details");
        }
    }
}
