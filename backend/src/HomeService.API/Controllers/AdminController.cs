using HomeService.Application.Commands.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : BaseApiController
{
    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new GetDashboardStatsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all users with admin filters
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] UserRole? role,
        [FromQuery] Region? region,
        [FromQuery] bool? isVerified,
        [FromQuery] DateTime? registeredAfter,
        [FromQuery] DateTime? registeredBefore,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminUsersQuery
        {
            SearchTerm = searchTerm,
            Role = role,
            Region = region,
            IsVerified = isVerified,
            RegisteredAfter = registeredAfter,
            RegisteredBefore = registeredBefore,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update user status (suspend/activate)
    /// </summary>
    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new UpdateUserStatusCommand
        {
            UserId = userId,
            IsSuspended = request.IsSuspended,
            Reason = request.Reason,
            AdminUserId = adminUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all bookings with admin filters
    /// </summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings(
        [FromQuery] BookingStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Region? region,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? providerId,
        [FromQuery] Guid? serviceId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isPaid,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminBookingsQuery
        {
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Region = region,
            CustomerId = customerId,
            ProviderId = providerId,
            ServiceId = serviceId,
            SearchTerm = searchTerm,
            IsPaid = isPaid,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get service catalog with admin capabilities
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetAllServices(
        [FromQuery] Guid? categoryId,
        [FromQuery] Region? region,
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminServicesQuery
        {
            CategoryId = categoryId,
            Region = region,
            IsActive = isActive,
            SearchTerm = searchTerm,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update service status
    /// </summary>
    [HttpPut("services/{serviceId}/status")]
    public async Task<IActionResult> UpdateServiceStatus(Guid serviceId, [FromBody] UpdateServiceStatusRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new UpdateServiceStatusCommand
        {
            ServiceId = serviceId,
            IsActive = request.IsActive,
            AdminUserId = adminUserId,
            Reason = request.Reason
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get financial analytics
    /// </summary>
    [HttpGet("analytics/financial")]
    [Authorize(Roles = "Admin,SuperAdmin,FinanceManager")]
    public async Task<IActionResult> GetFinancialAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Region? region)
    {
        var query = new GetFinancialAnalyticsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Region = region
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all transactions
    /// </summary>
    [HttpGet("transactions")]
    [Authorize(Roles = "Admin,SuperAdmin,FinanceManager")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] PaymentStatus? status,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] Region? region,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? providerId,
        [FromQuery] string? searchTerm,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAdminTransactionsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            PaymentMethod = paymentMethod,
            Region = region,
            CustomerId = customerId,
            ProviderId = providerId,
            SearchTerm = searchTerm,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Approve provider verification request
    /// </summary>
    [HttpPost("providers/{providerId}/verify")]
    public async Task<IActionResult> ApproveProviderVerification(Guid providerId, [FromBody] ApproveVerificationRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.ProviderVerification.ApproveVerificationCommand
        {
            ProviderId = providerId,
            AdminUserId = adminUserId,
            Notes = request.Notes
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reject provider verification request
    /// </summary>
    [HttpPost("providers/{providerId}/reject")]
    public async Task<IActionResult> RejectProviderVerification(Guid providerId, [FromBody] RejectVerificationRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.ProviderVerification.RejectVerificationCommand
        {
            ProviderId = providerId,
            AdminUserId = adminUserId,
            Reason = request.Reason
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record UpdateUserStatusRequest(bool IsSuspended, string? Reason);
public record UpdateServiceStatusRequest(bool IsActive, string? Reason);
public record ApproveVerificationRequest(string? Notes);
public record RejectVerificationRequest(string Reason);
