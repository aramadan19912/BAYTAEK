using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class SystemConfiguration : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } // If true, can be accessed by frontend
}
