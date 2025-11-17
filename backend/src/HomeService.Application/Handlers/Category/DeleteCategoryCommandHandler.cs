// DISABLED: Category entity does not exist in Domain layer
// using HomeService.Application.Commands.Category;
// using HomeService.Domain.Interfaces;
// using HomeService.Application.Common;
// using HomeService.Domain.Interfaces;
// using HomeService.Application.Interfaces;
// using HomeService.Domain.Interfaces;
// using MediatR;
// using HomeService.Domain.Interfaces;
// using Microsoft.Extensions.Logging;
// using HomeService.Domain.Interfaces;
// 
// namespace HomeService.Application.Handlers.Category;
// 
// public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
// {
//     private readonly IRepository<Domain.Entities.Service> _serviceRepository;
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ILogger<DeleteCategoryCommandHandler> _logger;
// 
//     public DeleteCategoryCommandHandler(
//         IRepository<Domain.Entities.Service> serviceRepository,
//         IUnitOfWork unitOfWork,
//         ILogger<DeleteCategoryCommandHandler> logger)
//     {
//         _serviceRepository = serviceRepository;
//         _unitOfWork = unitOfWork;
//         _logger = logger;
//     }
// 
//     public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
//     {
//         try
//         {
//             // Get category
//             if (category == null)
//             {
//                 return Result<bool>.Failure("Category not found");
//             }
// 
//             // Check if category has any services
//             var servicesInCategory = await _serviceRepository.FindAsync(
//                 s => s.CategoryId == request.CategoryId,
//                 cancellationToken);
// 
//             if (servicesInCategory != null && servicesInCategory.Any())
//             {
//                 var serviceCount = servicesInCategory.Count();
//                 return Result<bool>.Failure(
//                     $"Cannot delete category. It has {serviceCount} service(s) associated with it. " +
//                     "Please reassign or delete the services first.");
//             }
// 
//             // Delete category
//             await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//             _logger.LogInformation("Category {CategoryId} ({NameEn}) deleted by admin {AdminId}",
//                 request.CategoryId, category.NameEn, request.AdminUserId);
// 
//             return Result<bool>.Success(true, "Category deleted successfully");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error deleting category {CategoryId}", request.CategoryId);
//             return Result<bool>.Failure("An error occurred while deleting the category");
//         }
//     }
// }
// 
