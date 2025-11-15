using HomeService.Application.Commands.ProviderService;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.ProviderService;

public class UpdateProviderServiceCommandHandler : IRequestHandler<UpdateProviderServiceCommand, Result<bool>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProviderServiceCommandHandler> _logger;

    public UpdateProviderServiceCommandHandler(
        IRepository<Service> serviceRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProviderServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateProviderServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get service
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result<bool>.Failure("Service not found");
            }

            // Verify provider owns this service
            if (service.ProviderId != request.ProviderId)
            {
                _logger.LogWarning("Provider {ProviderId} attempted to update service {ServiceId} they don't own",
                    request.ProviderId, request.ServiceId);
                return Result<bool>.Failure("You are not authorized to update this service");
            }

            // Track changes
            var hasChanges = false;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.NameEn) && request.NameEn != service.NameEn)
            {
                service.NameEn = request.NameEn.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.NameAr) && request.NameAr != service.NameAr)
            {
                service.NameAr = request.NameAr.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.DescriptionEn) && request.DescriptionEn != service.DescriptionEn)
            {
                service.DescriptionEn = request.DescriptionEn.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.DescriptionAr) && request.DescriptionAr != service.DescriptionAr)
            {
                service.DescriptionAr = request.DescriptionAr.Trim();
                hasChanges = true;
            }

            if (request.BasePrice.HasValue && request.BasePrice.Value > 0 && request.BasePrice.Value != service.BasePrice)
            {
                service.BasePrice = request.BasePrice.Value;
                hasChanges = true;
            }

            if (request.EstimatedDurationMinutes.HasValue && request.EstimatedDurationMinutes.Value > 0
                && request.EstimatedDurationMinutes.Value != service.EstimatedDurationMinutes)
            {
                service.EstimatedDurationMinutes = request.EstimatedDurationMinutes.Value;
                hasChanges = true;
            }

            if (request.AvailableRegions != null && request.AvailableRegions.Any())
            {
                service.AvailableRegions = request.AvailableRegions.ToArray();
                hasChanges = true;
            }

            if (request.RequiredMaterials != null)
            {
                service.RequiredMaterials = string.IsNullOrWhiteSpace(request.RequiredMaterials)
                    ? null
                    : request.RequiredMaterials.Trim();
                hasChanges = true;
            }

            if (request.WarrantyInfo != null)
            {
                service.WarrantyInfo = string.IsNullOrWhiteSpace(request.WarrantyInfo)
                    ? null
                    : request.WarrantyInfo.Trim();
                hasChanges = true;
            }

            if (request.ImageUrls != null)
            {
                service.ImageUrls = request.ImageUrls.Any() ? request.ImageUrls.ToArray() : null;
                hasChanges = true;
            }

            if (request.VideoUrl != null)
            {
                service.VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl)
                    ? null
                    : request.VideoUrl.Trim();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return Result<bool>.Success(true, "No changes detected");
            }

            // Update audit fields
            service.UpdatedAt = DateTime.UtcNow;
            service.UpdatedBy = request.ProviderId.ToString();

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service {ServiceId} updated by provider {ProviderId}",
                request.ServiceId, request.ProviderId);

            return Result<bool>.Success(true, "Service updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service {ServiceId}", request.ServiceId);
            return Result<bool>.Failure("An error occurred while updating the service");
        }
    }
}
