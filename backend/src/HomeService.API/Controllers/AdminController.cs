using HomeService.Application.Commands.Admin;
using HomeService.Application.Features.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    /// <summary>
    /// Get all support tickets (Admin)
    /// </summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetAllTickets(
        [FromQuery] HomeService.Domain.Enums.TicketStatus? status,
        [FromQuery] HomeService.Domain.Enums.TicketCategory? category,
        [FromQuery] HomeService.Domain.Enums.TicketPriority? priority,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new Application.Queries.SupportTicket.GetAdminTicketsQuery
        {
            Status = status,
            Category = category,
            Priority = priority,
            UserId = userId,
            AssignedToUserId = assignedToUserId,
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
    /// Update ticket status (Admin)
    /// </summary>
    [HttpPut("tickets/{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(Guid ticketId, [FromBody] UpdateTicketStatusRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.SupportTicket.UpdateTicketStatusCommand
        {
            TicketId = ticketId,
            AdminUserId = adminUserId,
            Status = request.Status,
            Notes = request.Notes,
            AssignToUserId = request.AssignToUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Add admin response to ticket
    /// </summary>
    [HttpPost("tickets/{ticketId}/messages")]
    public async Task<IActionResult> AddAdminTicketMessage(Guid ticketId, [FromBody] AddAdminTicketMessageRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.SupportTicket.AddTicketMessageCommand
        {
            TicketId = ticketId,
            UserId = adminUserId,
            Message = request.Message,
            Attachments = request.Attachments,
            IsFromUser = false
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create content (Admin)
    /// </summary>
    [HttpPost("content")]
    public async Task<IActionResult> CreateContent([FromBody] CreateContentRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Content.CreateContentCommand
        {
            AdminUserId = adminUserId,
            Type = request.Type,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            ContentEn = request.ContentEn,
            ContentAr = request.ContentAr,
            Slug = request.Slug,
            Category = request.Category,
            Tags = request.Tags,
            MetaDescriptionEn = request.MetaDescriptionEn,
            MetaDescriptionAr = request.MetaDescriptionAr,
            IsPublished = request.IsPublished,
            DisplayOrder = request.DisplayOrder
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAllContent), new { }, result);
    }

    /// <summary>
    /// Update content (Admin)
    /// </summary>
    [HttpPut("content/{contentId}")]
    public async Task<IActionResult> UpdateContent(Guid contentId, [FromBody] UpdateContentRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Content.UpdateContentCommand
        {
            ContentId = contentId,
            AdminUserId = adminUserId,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            ContentEn = request.ContentEn,
            ContentAr = request.ContentAr,
            Slug = request.Slug,
            Category = request.Category,
            Tags = request.Tags,
            MetaDescriptionEn = request.MetaDescriptionEn,
            MetaDescriptionAr = request.MetaDescriptionAr,
            IsPublished = request.IsPublished,
            DisplayOrder = request.DisplayOrder
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete content (Admin)
    /// </summary>
    [HttpDelete("content/{contentId}")]
    public async Task<IActionResult> DeleteContent(Guid contentId)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Content.DeleteContentCommand
        {
            ContentId = contentId,
            AdminUserId = adminUserId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all content (Admin - includes unpublished)
    /// </summary>
    [HttpGet("content")]
    public async Task<IActionResult> GetAllContent(
        [FromQuery] HomeService.Domain.Enums.ContentType? type,
        [FromQuery] string? category,
        [FromQuery] bool? isPublished,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new Application.Queries.Content.GetContentByTypeQuery
        {
            Type = type ?? HomeService.Domain.Enums.ContentType.FAQ,
            Category = category,
            IsPublished = isPublished,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all reports (Admin)
    /// </summary>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] HomeService.Domain.Enums.ReportStatus? status,
        [FromQuery] HomeService.Domain.Enums.ReportType? type,
        [FromQuery] HomeService.Domain.Enums.ReportReason? reason,
        [FromQuery] Guid? reporterId,
        [FromQuery] Guid? reportedUserId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new Application.Queries.Report.GetReportsQuery
        {
            Status = status,
            Type = type,
            Reason = reason,
            ReporterId = reporterId,
            ReportedUserId = reportedUserId,
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
    /// Get report details (Admin)
    /// </summary>
    [HttpGet("reports/{reportId}")]
    public async Task<IActionResult> GetReportDetail(Guid reportId)
    {
        var query = new Application.Queries.Report.GetReportDetailQuery
        {
            ReportId = reportId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Review and take action on a report (Admin)
    /// </summary>
    [HttpPut("reports/{reportId}/review")]
    public async Task<IActionResult> ReviewReport(Guid reportId, [FromBody] ReviewReportRequest request)
    {
        var adminUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Report.ReviewReportCommand
        {
            ReportId = reportId,
            AdminUserId = adminUserId,
            Status = request.Status,
            ActionTaken = request.ActionTaken,
            AdminNotes = request.AdminNotes,
            SuspendUser = request.SuspendUser,
            SuspensionDays = request.SuspensionDays,
            SendWarning = request.SendWarning,
            WarningMessage = request.WarningMessage
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #region New Admin Dashboard Features

    /// <summary>
    /// Get platform analytics and KPIs
    /// </summary>
    [HttpGet("analytics/platform")]
    public async Task<IActionResult> GetPlatformAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetPlatformAnalyticsQuery(startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Manage user (suspend, activate, verify, unverify, delete)
    /// </summary>
    [HttpPost("users/{userId:guid}/manage")]
    public async Task<IActionResult> ManageUser(
        Guid userId,
        [FromBody] ManageUserRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId == Guid.Empty)
            return Unauthorized();

        var command = new ManageUserCommand(userId, adminUserId, request.Action, request.Reason);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Manage provider (verify, unverify, suspend, activate)
    /// </summary>
    [HttpPost("providers/{providerId:guid}/manage")]
    public async Task<IActionResult> ManageProvider(
        Guid providerId,
        [FromBody] ManageProviderRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId == Guid.Empty)
            return Unauthorized();

        var command = new ManageProviderCommand(providerId, adminUserId, request.Action, request.Reason);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get disputes for admin review
    /// </summary>
    [HttpGet("disputes")]
    public async Task<IActionResult> GetDisputes(
        [FromQuery] DisputeStatus? status,
        [FromQuery] DisputePriority? priority,
        [FromQuery] Guid? assignedTo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDisputesQuery(status, priority, assignedTo, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Manage dispute (assign, update status, resolve, close, escalate)
    /// </summary>
    [HttpPost("disputes/{disputeId:guid}/manage")]
    public async Task<IActionResult> ManageDispute(
        Guid disputeId,
        [FromBody] ManageDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId == Guid.Empty)
            return Unauthorized();

        var command = new ManageDisputeCommand(
            disputeId,
            adminUserId,
            request.Action,
            request.NewStatus,
            request.NewPriority,
            request.AssignTo,
            request.Resolution
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get financial report
    /// </summary>
    [HttpGet("financial/report")]
    [Authorize(Roles = "Admin,SuperAdmin,FinanceManager")]
    public async Task<IActionResult> GetFinancialReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetFinancialReportQuery(startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get system configurations
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetSystemConfigs(
        [FromQuery] string? category,
        [FromQuery] bool onlyPublic = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSystemConfigsQuery(category, onlyPublic);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update or create system configuration
    /// </summary>
    [HttpPost("config")]
    public async Task<IActionResult> ManageSystemConfig(
        [FromBody] ManageSystemConfigRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId == Guid.Empty)
            return Unauthorized();

        var command = new ManageSystemConfigCommand(
            request.Key,
            request.Value,
            adminUserId,
            request.Category,
            request.Description,
            request.IsPublic
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}

public record UpdateUserStatusRequest(bool IsSuspended, string? Reason);
public record UpdateServiceStatusRequest(bool IsActive, string? Reason);
public record ApproveVerificationRequest(string? Notes);
public record RejectVerificationRequest(string Reason);
public record UpdateTicketStatusRequest(
    HomeService.Domain.Enums.TicketStatus Status,
    string? Notes,
    Guid? AssignToUserId);
public record AddAdminTicketMessageRequest(
    string Message,
    List<string>? Attachments);
public record CreateContentRequest(
    HomeService.Domain.Enums.ContentType Type,
    string TitleEn,
    string TitleAr,
    string ContentEn,
    string ContentAr,
    string? Slug,
    string? Category,
    List<string>? Tags,
    string? MetaDescriptionEn,
    string? MetaDescriptionAr,
    bool IsPublished,
    int DisplayOrder);
public record UpdateContentRequest(
    string? TitleEn,
    string? TitleAr,
    string? ContentEn,
    string? ContentAr,
    string? Slug,
    string? Category,
    List<string>? Tags,
    string? MetaDescriptionEn,
    string? MetaDescriptionAr,
    bool? IsPublished,
    int? DisplayOrder);
public record ReviewReportRequest(
    HomeService.Domain.Enums.ReportStatus Status,
    HomeService.Domain.Enums.ModerationAction? ActionTaken,
    string? AdminNotes,
    bool SuspendUser,
    int? SuspensionDays,
    bool SendWarning,
    string? WarningMessage);

// New Admin Dashboard DTOs
public record ManageUserRequest(
    UserManagementAction Action,
    string? Reason = null);

public record ManageProviderRequest(
    ProviderManagementAction Action,
    string? Reason = null);

public record ManageDisputeRequest(
    DisputeManagementAction Action,
    DisputeStatus? NewStatus = null,
    DisputePriority? NewPriority = null,
    Guid? AssignTo = null,
    string? Resolution = null);

public record ManageSystemConfigRequest(
    string Key,
    string Value,
    string? Category = null,
    string? Description = null,
    bool IsPublic = false);
