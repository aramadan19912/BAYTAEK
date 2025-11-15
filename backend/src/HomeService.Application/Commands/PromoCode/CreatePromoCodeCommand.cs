using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.PromoCode;

public class CreatePromoCodeCommand : IRequest<Result<PromoCodeCreatedDto>>
{
    public Guid AdminUserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }

    // Discount type
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; } // For percentage discounts

    // Validity
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }

    // Usage limits
    public int? MaxTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public decimal? MinimumOrderAmount { get; set; }

    // Restrictions
    public List<Guid>? ApplicableServiceIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
    public List<Region>? ApplicableRegions { get; set; }
    public bool IsForFirstOrderOnly { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PromoCodeCreatedDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
