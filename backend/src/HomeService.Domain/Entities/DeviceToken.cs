using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class DeviceToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceModel { get; set; }
    public string? AppVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }

    // Alias properties for backward compatibility
    public DevicePlatform DeviceType
    {
        get => Platform;
        set => Platform = value;
    }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
