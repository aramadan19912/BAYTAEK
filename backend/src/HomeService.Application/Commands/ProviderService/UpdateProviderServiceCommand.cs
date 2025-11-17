using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.ProviderService;

public class UpdateProviderServiceCommand : IRequest<Result<bool>>
{
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }

    // Optional updates
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal? BasePrice { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public List<Region>? AvailableRegions { get; set; }
    public string? RequiredMaterials { get; set; }
    public string? WarrantyInfo { get; set; }
    public List<string>? ImageUrls { get; set; }
    public string? VideoUrl { get; set; }
}
