using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Provider;

public class GetProviderActiveBookingsQueryHandler : IRequestHandler<GetProviderActiveBookingsQuery, Result<List<ProviderBookingDto>>>
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ILogger<GetProviderActiveBookingsQueryHandler> _logger;

    public GetProviderActiveBookingsQueryHandler(
        IRepository<Booking> bookingRepository,
        IRepository<User> userRepository,
        IRepository<Service> serviceRepository,
        IRepository<Address> addressRepository,
        IRepository<Payment> paymentRepository,
        ILogger<GetProviderActiveBookingsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _serviceRepository = serviceRepository;
        _addressRepository = addressRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<List<ProviderBookingDto>>> Handle(GetProviderActiveBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get active bookings for the provider
            // Active means: Pending, Confirmed, InProgress (not Completed or Cancelled)
            var activeStatuses = new[]
            {
                BookingStatus.Pending,
                BookingStatus.Confirmed,
                BookingStatus.InProgress
            };

            var bookings = await _bookingRepository.FindAsync(
                b => b.ProviderId == request.ProviderId && activeStatuses.Contains(b.Status),
                cancellationToken);

            var bookingsList = bookings?.OrderBy(b => b.ScheduledAt).ToList() ?? new List<Booking>();

            if (!bookingsList.Any())
            {
                return Result<List<ProviderBookingDto>>.Success(new List<ProviderBookingDto>(), "No active bookings found");
            }

            // Get all related data
            var customerIds = bookingsList.Select(b => b.CustomerId).Distinct().ToList();
            var serviceIds = bookingsList.Select(b => b.ServiceId).Distinct().ToList();
            var addressIds = bookingsList.Select(b => b.AddressId).Distinct().ToList();
            var bookingIds = bookingsList.Select(b => b.Id).ToList();

            // Fetch related entities
            var customers = await _userRepository.FindAsync(u => customerIds.Contains(u.Id), cancellationToken);
            var services = await _serviceRepository.FindAsync(s => serviceIds.Contains(s.Id), cancellationToken);
            var addresses = await _addressRepository.FindAsync(a => addressIds.Contains(a.Id), cancellationToken);
            var payments = await _paymentRepository.FindAsync(p => bookingIds.Contains(p.BookingId), cancellationToken);

            var customerDict = customers?.ToDictionary(c => c.Id) ?? new Dictionary<Guid, User>();
            var serviceDict = services?.ToDictionary(s => s.Id) ?? new Dictionary<Guid, Service>();
            var addressDict = addresses?.ToDictionary(a => a.Id) ?? new Dictionary<Guid, Address>();
            var paymentDict = payments?
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.BookingId)
                .ToDictionary(g => g.Key, g => g.First())
                ?? new Dictionary<Guid, Payment>();

            // Map to DTOs
            var dtos = bookingsList.Select(booking =>
            {
                customerDict.TryGetValue(booking.CustomerId, out var customer);
                serviceDict.TryGetValue(booking.ServiceId, out var service);
                addressDict.TryGetValue(booking.AddressId, out var address);
                var isPaid = paymentDict.ContainsKey(booking.Id);

                return new ProviderBookingDto
                {
                    Id = booking.Id,
                    Status = booking.Status.ToString(),

                    CustomerId = booking.CustomerId,
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer",
                    CustomerPhone = customer?.PhoneNumber ?? string.Empty,
                    CustomerProfileImage = customer?.ProfileImageUrl,

                    ServiceId = booking.ServiceId,
                    ServiceName = service?.NameEn ?? "Unknown Service",

                    Address = address?.Street ?? "Unknown Address",
                    Latitude = address?.Latitude,
                    Longitude = address?.Longitude,

                    ScheduledAt = booking.ScheduledAt,
                    StartedAt = booking.StartedAt,
                    CompletedAt = booking.CompletedAt,

                    TotalAmount = booking.TotalAmount,
                    Currency = booking.Currency,

                    IsPaid = isPaid,

                    SpecialInstructions = booking.SpecialInstructions,

                    CreatedAt = booking.CreatedAt
                };
            }).ToList();

            _logger.LogInformation("Retrieved {Count} active bookings for provider {ProviderId}",
                dtos.Count, request.ProviderId);

            return Result<List<ProviderBookingDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active bookings for provider {ProviderId}", request.ProviderId);
            return Result<List<ProviderBookingDto>>.Failure("An error occurred while retrieving active bookings");
        }
    }
}
