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
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly ILogger<GetBookingDetailQueryHandler> _logger;

    public GetBookingDetailQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<HomeService.Domain.Entities.User> userRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IRepository<HomeService.Domain.Entities.Payment> paymentRepository,
        IRepository<HomeService.Domain.Entities.Review> reviewRepository,
        IRepository<ServiceCategory> categoryRepository,
        ILogger<GetBookingDetailQueryHandler> logger)
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
            var provider = booking.ProviderId.HasValue ? await _providerRepository.GetByIdAsync(booking.ProviderId.Value, cancellationToken) : null;
            var address = await _addressRepository.GetByIdAsync(booking.AddressId, cancellationToken);

            Domain.Entities.User? providerUser = null;
            if (provider != null)
            {
                providerUser = await _userRepository.GetByIdAsync(provider.UserId, cancellationToken);
            }

            ServiceCategory? category = null;
            if (service != null)
            {
                category = await _categoryRepository.GetByIdAsync(service.CategoryId, cancellationToken);
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

            // Confirmed status would be tracked separately if needed
            // if (booking.AcceptedAt.HasValue)
            // {
            //     timeline.Add(new BookingTimelineDto
            //     {
            //         Status = BookingStatus.Confirmed,
            //         Timestamp = booking.AcceptedAt.Value,
            //         Notes = "Booking confirmed by provider"
            //     });
            // }

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
                BookingNumber = $"BK-{booking.Id.ToString().Substring(0, 8).ToUpper()}",
                Status = booking.Status,
                ScheduledDateTime = booking.ScheduledAt,
                CreatedAt = booking.CreatedAt,
                ConfirmedAt = booking.CreatedAt,
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
                    ImageUrl = service?.ImageUrls?.FirstOrDefault(),
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
                    YearsOfExperience = 0 // YearsOfExperience doesn't exist in ServiceProvider
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
                    ServicePrice = service?.BasePrice ?? booking.TotalAmount,
                    TaxAmount = booking.VatAmount,
                    DiscountAmount = booking.DiscountAmount ?? 0m,
                    TotalAmount = booking.TotalAmount,
                    Currency = booking.Currency.ToString()
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
                    Images = review.ImageUrls?.ToList() ?? new List<string>(),
                    CreatedAt = review.CreatedAt,
                    ProviderResponse = review.ProviderResponse,
                    ProviderRespondedAt = review.ProviderRespondedAt
                } : null,

                // Timeline
                Timeline = timeline,

                // Notes
                CustomerNotes = booking.SpecialInstructions,
                ProviderNotes = booking.ProviderNotes,
                AdminNotes = null, // AdminNotes property doesn't exist

                // Photos
                BeforePhotos = new List<string>(), // BeforePhotos not in Booking entity
                AfterPhotos = booking.CompletionPhotos?.Split(',').ToList() ?? new List<string>(),

                // Cancellation info
                CancellationReason = booking.CancellationReason,
                RefundAmount = null, // RefundAmount on Payment, not Booking
                RefundStatus = null // RefundStatus on Payment, not Booking
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
