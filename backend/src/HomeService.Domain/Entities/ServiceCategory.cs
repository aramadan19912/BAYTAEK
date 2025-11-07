using HomeService.Domain.Common;

namespace HomeService.Domain.Entities;

public class ServiceCategory : AuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }

    // Navigation properties
    public virtual ServiceCategory? ParentCategory { get; set; }
    public virtual ICollection<ServiceCategory> SubCategories { get; set; } = new List<ServiceCategory>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
