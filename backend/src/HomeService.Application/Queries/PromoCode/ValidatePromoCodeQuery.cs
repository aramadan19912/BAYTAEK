using HomeService.Application.Common.Models;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Queries.PromoCode;

public class ValidatePromoCodeQuery : IRequest<Result<PromoCodeValidationDto>>
{
    public string Code { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal OrderAmount { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? CategoryId { get; set; }
    public Region? Region { get; set; }
}

public class PromoCodeValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    // If valid, provide discount details
    public Guid? PromoCodeId { get; set; }
    public string? Code { get; set; }
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? CalculatedDiscountAmount { get; set; }
    public decimal? FinalAmount { get; set; }
}
