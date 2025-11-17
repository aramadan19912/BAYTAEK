using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Admin;

public record GetSystemConfigsQuery(
    string? Category = null,
    bool OnlyPublic = false
) : IRequest<Result<List<SystemConfigDto>>>;

public class GetSystemConfigsQueryHandler
    : IRequestHandler<GetSystemConfigsQuery, Result<List<SystemConfigDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSystemConfigsQueryHandler> _logger;

    public GetSystemConfigsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSystemConfigsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<SystemConfigDto>>> Handle(
        GetSystemConfigsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable();

            if (!string.IsNullOrEmpty(request.Category))
                query = query.Where(c => c.Category == request.Category);

            if (request.OnlyPublic)
                query = query.Where(c => c.IsPublic);

            var configs = await query
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Key)
                .ToListAsync(cancellationToken);

            var configDtos = configs.Select(c => new SystemConfigDto
            {
                Id = c.Id,
                Key = c.Key,
                Value = c.Value,
                Category = c.Category,
                Description = c.Description,
                IsPublic = c.IsPublic,
                UpdatedAt = c.UpdatedAt ?? c.CreatedAt
            }).ToList();

            return Result.Success(configDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system configurations");
            return Result.Failure<List<SystemConfigDto>>(
                "An error occurred while retrieving system configurations",
                ex.Message);
        }
    }
}
