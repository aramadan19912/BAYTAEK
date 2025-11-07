using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Queries.User;

public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
{
    public Guid UserId { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Provider-specific fields (null for customers)
    public ProviderProfileDto? ProviderProfile { get; set; }

    // Statistics
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
}

public class ProviderProfileDto
{
    public Guid ProviderId { get; set; }
    public string? BusinessName { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedBookings { get; set; }
    public bool IsVerified { get; set; }
    public string? LicenseNumber { get; set; }
    public List<string> CertificationDocuments { get; set; } = new();
    public List<string> PortfolioImages { get; set; } = new();
    public List<ServiceCategoryDto> ServiceCategories { get; set; } = new();
}

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}
