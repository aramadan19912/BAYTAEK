using HomeService.Domain.Common;
using HomeService.Domain.Enums;

namespace HomeService.Domain.Entities;

public class PromoCode : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUsageCount { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? CategoryId { get; set; }
    public Region? ApplicableRegion { get; set; }

    // Navigation properties
    public virtual Service? Service { get; set; }
    public virtual ServiceCategory? Category { get; set; }
    public virtual ICollection<PromoCodeUsage> Usages { get; set; } = new List<PromoCodeUsage>();
}
