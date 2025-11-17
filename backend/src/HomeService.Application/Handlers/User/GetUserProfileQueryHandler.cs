using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.User;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.User;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IRepository<Domain.Entities.User> _userRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Domain.Entities.Booking> _bookingRepository;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IRepository<Domain.Entities.User> userRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<Domain.Entities.Booking> bookingRepository,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _providerRepository = providerRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<UserProfileDto>.Failure("User not found");
            }

            // Get booking statistics
            var allBookings = await _bookingRepository.FindAsync(
                b => b.CustomerId == request.UserId || b.ProviderId == request.UserId,
                cancellationToken);

            var bookingsList = allBookings?.ToList() ?? new List<Domain.Entities.Booking>();
            var totalBookings = bookingsList.Count;
            var completedBookings = bookingsList.Count(b => b.Status == BookingStatus.Completed);
            var cancelledBookings = bookingsList.Count(b => b.Status == BookingStatus.Cancelled);

            // Build base profile DTO
            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Region = user.Region.ToString(),
                PreferredLanguage = user.PreferredLanguage,
                ProfileImageUrl = user.ProfileImageUrl,
                EmailVerified = user.EmailVerified,
                PhoneVerified = user.PhoneVerified,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings
            };

            // If user is a provider, add provider-specific information
            if (user.Role == UserRole.ServiceProvider)
            {
                var providerProfile = await _providerRepository.FindAsync(
                    p => p.UserId == request.UserId,
                    cancellationToken);

                var provider = providerProfile?.FirstOrDefault();
                if (provider != null)
                {
                    profileDto.ProviderProfile = new ProviderProfileDto
                    {
                        ProviderId = provider.Id,
                        BusinessName = provider.BusinessName,
                        AverageRating = provider.AverageRating,
                        TotalReviews = provider.TotalReviews,
                        CompletedBookings = provider.CompletedBookings,
                        IsVerified = provider.IsVerified,
                        LicenseNumber = provider.LicenseNumber,
                        CertificationDocuments = provider.CertificationDocuments?.ToList() ?? new List<string>(),
                        PortfolioImages = provider.PortfolioImages?.ToList() ?? new List<string>(),
                        ServiceCategories = new List<ServiceCategoryDto>() // TODO: Implement service categories relationship
                    };
                }
            }

            _logger.LogInformation("Retrieved profile for user {UserId}", request.UserId);

            return Result<UserProfileDto>.Success(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user {UserId}", request.UserId);
            return Result<UserProfileDto>.Failure("An error occurred while retrieving the user profile");
        }
    }
}
