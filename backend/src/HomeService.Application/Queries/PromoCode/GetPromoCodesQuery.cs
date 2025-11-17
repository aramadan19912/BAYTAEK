using HomeService.Application.Common;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.PromoCode;

public class GetPromoCodesQuery : IRequest<Result<PromoCodesListDto>>
{
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PromoCodesListDto
{
    public List<PromoCodeDto> PromoCodes { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PromoCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }

    // Discount info
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    // Validity
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsCurrentlyValid { get; set; }

    // Usage
    public int? MaxTotalUses { get; set; }
    public int CurrentTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public decimal? MinimumOrderAmount { get; set; }

    // Restrictions
    public int? ApplicableServicesCount { get; set; }
    public int? ApplicableCategoriesCount { get; set; }
    public List<string>? ApplicableRegions { get; set; }
    public bool IsForFirstOrderOnly { get; set; }

    // Status
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
