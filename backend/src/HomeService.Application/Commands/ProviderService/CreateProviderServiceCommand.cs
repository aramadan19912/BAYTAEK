using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.ProviderService;

public class CreateProviderServiceCommand : IRequest<Result<ServiceCreatedDto>>
{
    public Guid ProviderId { get; set; }
    public Guid CategoryId { get; set; }

    // Service details (EN)
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    // Service details (AR)
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    // Pricing
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "SAR";

    // Service info
    public int EstimatedDurationMinutes { get; set; }
    public List<Region> AvailableRegions { get; set; } = new();

    // Additional details
    public string? RequiredMaterials { get; set; }
    public string? WarrantyInfo { get; set; }

    // Media
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
}

public class ServiceCreatedDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = string.Empty;
}
