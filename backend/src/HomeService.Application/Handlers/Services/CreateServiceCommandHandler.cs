using AutoMapper;
using HomeService.Application.Commands.Services;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Services;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateServiceCommandHandler> _logger;

    public CreateServiceCommandHandler(
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<ServiceCategory> categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ServiceDto>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result.Failure<ServiceDto>("Service category not found");
            }

            var service = new HomeService.Domain.Entities.Service
            {
                Id = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                DescriptionEn = request.DescriptionEn,
                DescriptionAr = request.DescriptionAr,
                BasePrice = request.BasePrice,
                Currency = request.Currency,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                IsFeatured = request.IsFeatured,
                AvailableRegions = request.AvailableRegions,
                ImageUrls = request.ImageUrls,
                VideoUrl = request.VideoUrl,
                RequiredMaterials = request.RequiredMaterials,
                WarrantyInfo = request.WarrantyInfo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _serviceRepository.AddAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var serviceDto = _mapper.Map<ServiceDto>(service);

            _logger.LogInformation("Service created successfully: {ServiceName}", service.NameEn);

            return Result.Success(serviceDto, "Service created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service");
            return Result.Failure<ServiceDto>("An error occurred while creating service", ex.Message);
        }
    }
}
