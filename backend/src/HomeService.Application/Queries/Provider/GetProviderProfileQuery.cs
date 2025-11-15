using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.Provider;

public class GetProviderProfileQuery : IRequest<Result<ProviderProfileDetailDto>>
{
    public Guid ProviderId { get; set; }
}

public class ProviderProfileDetailDto
{
    // Basic Provider Info
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    // Business Info
    public string? BusinessName { get; set; }
    public string? LicenseNumber { get; set; }
    public bool IsVerified { get; set; }

    // Performance Metrics
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedBookings { get; set; }
    public int TotalBookings { get; set; }
    public int CancelledBookings { get; set; }
    public decimal CompletionRate { get; set; }

    // Availability
    public bool IsOnline { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }

    // Documents & Portfolio
    public List<string> CertificationDocuments { get; set; } = new();
    public List<string> PortfolioImages { get; set; } = new();

    // Service Categories
    public List<ServiceCategoryDto> ServiceCategories { get; set; } = new();

    // Financial
    public decimal TotalEarnings { get; set; }
    public decimal PendingPayouts { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IbanNumber { get; set; } = string.Empty;

    // Account Settings
    public string PreferredLanguage { get; set; } = "en";
    public string Region { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class ServiceCategoryDto
{
    public Guid CategoryId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
}
