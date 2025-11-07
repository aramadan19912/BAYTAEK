using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.Category;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Category;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IRepository<Domain.Entities.Category> _categoryRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        IRepository<Domain.Entities.Category> categoryRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all categories or filter by active status
            var categoriesQuery = request.IsActive.HasValue
                ? await _categoryRepository.FindAsync(c => c.IsActive == request.IsActive.Value, cancellationToken)
                : await _categoryRepository.FindAsync(c => true, cancellationToken);

            var categories = categoriesQuery?.OrderBy(c => c.NameEn).ToList() ?? new List<Domain.Entities.Category>();

            // Get service counts if requested
            Dictionary<Guid, int> serviceCounts = new();
            if (request.IncludeServiceCount)
            {
                var allServices = await _serviceRepository.FindAsync(s => s.IsActive, cancellationToken);
                serviceCounts = allServices?
                    .GroupBy(s => s.CategoryId)
                    .ToDictionary(g => g.Key, g => g.Count())
                    ?? new Dictionary<Guid, int>();
            }

            // Map to DTOs
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                DescriptionEn = c.DescriptionEn,
                DescriptionAr = c.DescriptionAr,
                IconUrl = c.IconUrl,
                IsActive = c.IsActive,
                ServiceCount = serviceCounts.GetValueOrDefault(c.Id, 0),
                CreatedAt = c.CreatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} categories", categoryDtos.Count);

            return Result<List<CategoryDto>>.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return Result<List<CategoryDto>>.Failure("An error occurred while retrieving categories");
        }
    }
}
