using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Application.Features.Admin;

public record ManageSystemConfigCommand(
    string Key,
    string Value,
    Guid AdminUserId,
    string? Category = null,
    string? Description = null,
    bool IsPublic = false
) : IRequest<Result<SystemConfigDto>>;

public class ManageSystemConfigCommandHandler
    : IRequestHandler<ManageSystemConfigCommand, Result<SystemConfigDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManageSystemConfigCommandHandler> _logger;

    public ManageSystemConfigCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ManageSystemConfigCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SystemConfigDto>> Handle(
        ManageSystemConfigCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if config exists
            var existingConfig = await _unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Key == request.Key, cancellationToken);

            if (existingConfig != null)
            {
                // Update existing
                existingConfig.Value = request.Value;
                if (!string.IsNullOrEmpty(request.Category))
                    existingConfig.Category = request.Category;
                if (!string.IsNullOrEmpty(request.Description))
                    existingConfig.Description = request.Description;
                existingConfig.IsPublic = request.IsPublic;
                existingConfig.UpdatedAt = DateTime.UtcNow;
                existingConfig.UpdatedBy = request.AdminUserId.ToString();

                _unitOfWork.Repository<SystemConfiguration>().Update(existingConfig);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("System config updated: {Key} by admin {AdminId}",
                    request.Key, request.AdminUserId);

                return Result.Success(MapToDto(existingConfig), "Configuration updated successfully");
            }
            else
            {
                // Create new
                var newConfig = new SystemConfiguration
                {
                    Key = request.Key,
                    Value = request.Value,
                    Category = request.Category ?? "General",
                    Description = request.Description ?? string.Empty,
                    IsPublic = request.IsPublic,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.AdminUserId.ToString()
                };

                await _unitOfWork.Repository<SystemConfiguration>().AddAsync(newConfig, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("System config created: {Key} by admin {AdminId}",
                    request.Key, request.AdminUserId);

                return Result.Success(MapToDto(newConfig), "Configuration created successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing system config {Key}", request.Key);
            return Result.Failure<SystemConfigDto>(
                "An error occurred while managing system configuration",
                ex.Message);
        }
    }

    private SystemConfigDto MapToDto(SystemConfiguration config)
    {
        return new SystemConfigDto
        {
            Id = config.Id,
            Key = config.Key,
            Value = config.Value,
            Category = config.Category,
            Description = config.Description,
            IsPublic = config.IsPublic,
            UpdatedAt = config.UpdatedAt ?? config.CreatedAt
        };
    }
}

public class SystemConfigDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime UpdatedAt { get; set; }
}
