using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Admin;

public record GetDisputesQuery(
    DisputeStatus? Status = null,
    DisputePriority? Priority = null,
    Guid? AssignedTo = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<DisputeDto>>>;

public class GetDisputesQueryHandler
    : IRequestHandler<GetDisputesQuery, Result<PagedResult<DisputeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDisputesQueryHandler> _logger;

    public GetDisputesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDisputesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<DisputeDto>>> Handle(
        GetDisputesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<Dispute>()
                .GetQueryable()
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Service)
                .Include(d => d.RaisedByUser)
                .Include(d => d.AssignedToUser)
                .AsQueryable();

            // Apply filters
            if (request.Status.HasValue)
                query = query.Where(d => d.Status == request.Status.Value);

            if (request.Priority.HasValue)
                query = query.Where(d => d.Priority == request.Priority.Value);

            if (request.AssignedTo.HasValue)
                query = query.Where(d => d.AssignedTo == request.AssignedTo.Value);

            // Order by priority and creation date
            query = query
                .OrderByDescending(d => d.Priority)
                .ThenByDescending(d => d.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var disputes = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var disputeDtos = disputes.Select(d => new DisputeDto
            {
                Id = d.Id,
                BookingId = d.BookingId,
                ServiceName = d.Booking.Service.NameEn,
                RaisedBy = d.RaisedBy,
                RaisedByName = $"{d.RaisedByUser.FirstName} {d.RaisedByUser.LastName}",
                Type = d.Type.ToString(),
                Title = d.Title,
                Description = d.Description,
                Status = d.Status.ToString(),
                Priority = d.Priority.ToString(),
                AssignedTo = d.AssignedTo,
                AssignedToName = d.AssignedToUser != null
                    ? $"{d.AssignedToUser.FirstName} {d.AssignedToUser.LastName}"
                    : null,
                Resolution = d.Resolution,
                ResolvedAt = d.ResolvedAt,
                EvidenceUrls = d.EvidenceUrls?.ToList() ?? new List<string>(),
                CreatedAt = d.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<DisputeDto>
            {
                Items = disputeDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting disputes");
            return Result.Failure<PagedResult<DisputeDto>>(
                "An error occurred while retrieving disputes",
                ex.Message);
        }
    }
}

public class DisputeDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public Guid RaisedBy { get; set; }
    public string RaisedByName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<string> EvidenceUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
