// DISABLED: Category entity does not exist in Domain layer
// using HomeService.Application.Common;
// using HomeService.Domain.Interfaces;
// using HomeService.Application.Interfaces;
// using HomeService.Domain.Interfaces;
// using HomeService.Application.Queries.Category;
// using HomeService.Domain.Interfaces;
// using MediatR;
// using HomeService.Domain.Interfaces;
// using Microsoft.Extensions.Logging;
// using HomeService.Domain.Interfaces;
// 
// namespace HomeService.Application.Handlers.Category;
// 
// public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
// {
//     private readonly IRepository<Domain.Entities.Service> _serviceRepository;
//     private readonly ILogger<GetCategoriesQueryHandler> _logger;
// 
//     public GetCategoriesQueryHandler(
//         IRepository<Domain.Entities.Service> serviceRepository,
//         ILogger<GetCategoriesQueryHandler> logger)
//     {
//         _serviceRepository = serviceRepository;
//         _logger = logger;
//     }
// 
//     public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
//     {
//         try
//         {
//             // Get all categories or filter by active status
//             var categoriesQuery = request.IsActive.HasValue
// 
// 
//             // Get service counts if requested
//             Dictionary<Guid, int> serviceCounts = new();
//             if (request.IncludeServiceCount)
//             {
//                 var allServices = await _serviceRepository.FindAsync(s => s.IsActive, cancellationToken);
//                 serviceCounts = allServices?
//                     .GroupBy(s => s.CategoryId)
//                     .ToDictionary(g => g.Key, g => g.Count())
//                     ?? new Dictionary<Guid, int>();
//             }
// 
//             // Map to DTOs
//             var categoryDtos = categories.Select(c => new CategoryDto
//             {
//                 Id = c.Id,
//                 NameEn = c.NameEn,
//                 NameAr = c.NameAr,
//                 DescriptionEn = c.DescriptionEn,
//                 DescriptionAr = c.DescriptionAr,
//                 IconUrl = c.IconUrl,
//                 IsActive = c.IsActive,
//                 ServiceCount = serviceCounts.GetValueOrDefault(c.Id, 0),
//                 CreatedAt = c.CreatedAt
//             }).ToList();
// 
//             _logger.LogInformation("Retrieved {Count} categories", categoryDtos.Count);
// 
//             return Result<List<CategoryDto>>.Success(categoryDtos);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error retrieving categories");
//             return Result<List<CategoryDto>>.Failure("An error occurred while retrieving categories");
//         }
//     }
// }
// 
