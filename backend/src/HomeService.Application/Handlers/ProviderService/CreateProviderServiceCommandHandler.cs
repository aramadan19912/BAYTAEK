using HomeService.Application.Commands.ProviderService;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.ProviderService;

public class CreateProviderServiceCommandHandler : IRequestHandler<CreateProviderServiceCommand, Result<ServiceCreatedDto>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProviderServiceCommandHandler> _logger;

    public CreateProviderServiceCommandHandler(
        IRepository<Service> serviceRepository,
        IRepository<ServiceProvider> providerRepository,
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateProviderServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServiceCreatedDto>> Handle(CreateProviderServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider exists
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<ServiceCreatedDto>.Failure("Provider not found");
            }

            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<ServiceCreatedDto>.Failure("Category not found");
            }

            if (!category.IsActive)
            {
                return Result<ServiceCreatedDto>.Failure("Category is not active");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.NameEn) || string.IsNullOrWhiteSpace(request.NameAr))
            {
                return Result<ServiceCreatedDto>.Failure("Service name in both English and Arabic is required");
            }

            if (string.IsNullOrWhiteSpace(request.DescriptionEn) || string.IsNullOrWhiteSpace(request.DescriptionAr))
            {
                return Result<ServiceCreatedDto>.Failure("Service description in both English and Arabic is required");
            }

            if (request.BasePrice <= 0)
            {
                return Result<ServiceCreatedDto>.Failure("Base price must be greater than zero");
            }

            if (request.EstimatedDurationMinutes <= 0)
            {
                return Result<ServiceCreatedDto>.Failure("Estimated duration must be greater than zero");
            }

            if (!request.AvailableRegions.Any())
            {
                return Result<ServiceCreatedDto>.Failure("At least one region must be selected");
            }

            // Create service
            var service = new Service
            {
                Id = Guid.NewGuid(),
                ProviderId = request.ProviderId,
                CategoryId = request.CategoryId,
                NameEn = request.NameEn.Trim(),
                NameAr = request.NameAr.Trim(),
                DescriptionEn = request.DescriptionEn.Trim(),
                DescriptionAr = request.DescriptionAr.Trim(),
                BasePrice = request.BasePrice,
                Currency = request.Currency,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                AvailableRegions = request.AvailableRegions.ToArray(),
                RequiredMaterials = request.RequiredMaterials?.Trim(),
                WarrantyInfo = request.WarrantyInfo?.Trim(),
                ImageUrls = request.ImageUrls?.ToArray(),
                VideoUrl = request.VideoUrl?.Trim(),
                IsActive = false, // Requires admin approval
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ProviderId.ToString()
            };

            await _serviceRepository.AddAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service created by provider {ProviderId}: {NameEn} - Pending approval",
                request.ProviderId, request.NameEn);

            var result = new ServiceCreatedDto
            {
                Id = service.Id,
                NameEn = service.NameEn,
                NameAr = service.NameAr,
                BasePrice = service.BasePrice,
                IsActive = service.IsActive,
                Message = "Service created successfully. It will be available after admin approval."
            };

            return Result<ServiceCreatedDto>.Success(result, "Service created successfully and pending approval");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service for provider {ProviderId}", request.ProviderId);
            return Result<ServiceCreatedDto>.Failure("An error occurred while creating the service");
        }
    }
}
