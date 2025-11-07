using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Services;

public record CreateServiceCommand : IRequest<Result<ServiceDto>>
{
    public Guid CategoryId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string DescriptionEn { get; init; } = string.Empty;
    public string DescriptionAr { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public Currency Currency { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public bool IsFeatured { get; init; }
    public Region[] AvailableRegions { get; init; } = Array.Empty<Region>();
    public string[] ImageUrls { get; init; } = Array.Empty<string>();
    public string? VideoUrl { get; init; }
    public string? RequiredMaterials { get; init; }
    public string? WarrantyInfo { get; init; }
}
