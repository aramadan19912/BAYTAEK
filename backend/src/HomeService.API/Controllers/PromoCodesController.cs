using HomeService.Application.Commands.PromoCode;
using HomeService.Application.Queries.PromoCode;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class PromoCodesController : BaseApiController
{
    /// <summary>
    /// Validate a promo code (Customer)
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidatePromoCode([FromBody] ValidatePromoCodeRequest request)
    {
        var customerId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new ValidatePromoCodeQuery
        {
            Code = request.Code,
            CustomerId = customerId,
            OrderAmount = request.OrderAmount,
            ServiceId = request.ServiceId,
            CategoryId = request.CategoryId,
            Region = request.Region
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all promo codes (Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetPromoCodes(
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPromoCodesQuery
        {
            IsActive = isActive,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new promo code (Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreatePromoCode([FromBody] CreatePromoCodeRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new CreatePromoCodeCommand
        {
            AdminUserId = adminUserId,
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            MaxTotalUses = request.MaxTotalUses,
            MaxUsesPerCustomer = request.MaxUsesPerCustomer,
            MinimumOrderAmount = request.MinimumOrderAmount,
            ApplicableServiceIds = request.ApplicableServiceIds,
            ApplicableCategoryIds = request.ApplicableCategoryIds,
            ApplicableRegions = request.ApplicableRegions,
            IsForFirstOrderOnly = request.IsForFirstOrderOnly,
            IsActive = request.IsActive
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetPromoCodes), new { }, result);
    }

    /// <summary>
    /// Update promo code status (Admin)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdatePromoCodeStatus(Guid id, [FromBody] UpdatePromoCodeStatusRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new UpdatePromoCodeStatusCommand
        {
            PromoCodeId = id,
            AdminUserId = adminUserId,
            IsActive = request.IsActive
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record ValidatePromoCodeRequest(
    string Code,
    decimal OrderAmount,
    Guid? ServiceId,
    Guid? CategoryId,
    Region? Region);

public record CreatePromoCodeRequest(
    string Code,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    DateTime ValidFrom,
    DateTime ValidUntil,
    int? MaxTotalUses,
    int? MaxUsesPerCustomer,
    decimal? MinimumOrderAmount,
    List<Guid>? ApplicableServiceIds,
    List<Guid>? ApplicableCategoryIds,
    List<Region>? ApplicableRegions,
    bool IsForFirstOrderOnly,
    bool IsActive);

public record UpdatePromoCodeStatusRequest(bool IsActive);
