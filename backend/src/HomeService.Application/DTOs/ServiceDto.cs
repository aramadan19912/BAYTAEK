using HomeService.Domain.Enums;

namespace HomeService.Application.DTOs;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public Currency Currency { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsFeatured { get; set; }
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
}
