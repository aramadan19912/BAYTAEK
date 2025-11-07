using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs.Admin;

public class AdminServiceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }

    // Category Information
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Regional Availability
    public Region Region { get; set; }
    public bool IsActive { get; set; }

    // Statistics
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal TotalRevenue { get; set; }

    // Provider Information
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
