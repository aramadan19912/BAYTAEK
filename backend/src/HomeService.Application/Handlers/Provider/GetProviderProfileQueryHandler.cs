using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.Provider;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.Provider;

public class GetProviderProfileQueryHandler : IRequestHandler<GetProviderProfileQuery, Result<ProviderProfileDetailDto>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<Domain.Entities.Payment> _paymentRepository;
    private readonly ILogger<GetProviderProfileQueryHandler> _logger;

    public GetProviderProfileQueryHandler(
        IRepository<Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.Booking> bookingRepository,
        IRepository<Domain.Entities.Payment> paymentRepository,
        ILogger<GetProviderProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<ProviderProfileDetailDto>> Handle(GetProviderProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider profile
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<ProviderProfileDetailDto>.Failure("Provider not found");
            }

            // Get associated user
            var user = await _userRepository.GetByIdAsync(provider.UserId, cancellationToken);
            if (user == null)
            {
                return Result<ProviderProfileDetailDto>.Failure("User account not found");
            }

            // Get booking statistics
            var allBookings = await _bookingRepository.FindAsync(
                b => b.ProviderId == request.ProviderId,
                cancellationToken);

            var bookingsList = allBookings?.ToList() ?? new List<Domain.Entities.Booking>();
            var totalBookings = bookingsList.Count;
            var completedBookings = bookingsList.Count(b => b.Status == BookingStatus.Completed);
            var cancelledBookings = bookingsList.Count(b => b.Status == BookingStatus.Cancelled);
            var completionRate = totalBookings > 0
                ? Math.Round((decimal)completedBookings / totalBookings * 100, 2)
                : 0;

            // Get earnings
            var completedBookingIds = bookingsList
                .Where(b => b.Status == BookingStatus.Completed)
                .Select(b => b.Id)
                .ToList();

            var payments = await _paymentRepository.FindAsync(
                p => completedBookingIds.Contains(p.BookingId) && p.Status == PaymentStatus.Completed,
                cancellationToken);

            var paymentsList = payments?.ToList() ?? new List<Domain.Entities.Payment>();
            var totalEarnings = paymentsList.Sum(p => p.Amount);

            // Calculate pending payouts (completed but not yet paid out)
            // TODO: This should check against a Payout entity when implemented
            var pendingPayouts = totalEarnings * 0.85m; // Assuming 85% after platform fee

            // Build profile DTO
            var profileDto = new ProviderProfileDetailDto
            {
                Id = provider.Id,
                UserId = provider.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,

                // Business Info
                BusinessName = provider.BusinessName,
                LicenseNumber = provider.LicenseNumber,
                IsVerified = provider.IsVerified,

                // Performance Metrics
                AverageRating = provider.AverageRating,
                TotalReviews = provider.TotalReviews,
                CompletedBookings = completedBookings,
                TotalBookings = totalBookings,
                CancelledBookings = cancelledBookings,
                CompletionRate = completionRate,

                // Availability
                IsOnline = provider.IsOnline,
                IsAvailable = provider.IsAvailable,
                AvailableFrom = provider.AvailableFrom,
                AvailableUntil = provider.AvailableUntil,

                // Documents & Portfolio
                CertificationDocuments = provider.CertificationDocuments?.Split(',').ToList() ?? new List<string>(),
                PortfolioImages = provider.PortfolioImages?.Split(',').ToList() ?? new List<string>(),

                // Service Categories (TODO: Implement when service categories relationship is ready)
                ServiceCategories = new List<ServiceCategoryDto>(),

                // Financial
                TotalEarnings = totalEarnings,
                PendingPayouts = pendingPayouts,
                BankAccountNumber = "", // Property doesn't exist in ServiceProvider
                BankName = "", // Property doesn't exist in ServiceProvider
                IbanNumber = "", // Property doesn't exist in ServiceProvider

                // Account Settings
                PreferredLanguage = user.PreferredLanguage.ToString(),
                Region = user.Region.ToString(),
                EmailVerified = user.EmailVerified,
                PhoneVerified = user.PhoneVerified,
                TwoFactorEnabled = user.TwoFactorEnabled,

                // Timestamps
                CreatedAt = provider.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            _logger.LogInformation("Retrieved profile for provider {ProviderId}", request.ProviderId);

            return Result<ProviderProfileDetailDto>.Success(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for provider {ProviderId}", request.ProviderId);
            return Result<ProviderProfileDetailDto>.Failure("An error occurred while retrieving the provider profile");
        }
    }
}
