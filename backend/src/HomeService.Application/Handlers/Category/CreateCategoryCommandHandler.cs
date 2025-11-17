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
// public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryCreatedDto>>
// {
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly ILogger<CreateCategoryCommandHandler> _logger;
// 
//     public CreateCategoryCommandHandler(
//         IUnitOfWork unitOfWork,
//         ILogger<CreateCategoryCommandHandler> logger)
//     {
//         _unitOfWork = unitOfWork;
//         _logger = logger;
//     }
// 
//     public async Task<Result<CategoryCreatedDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
//     {
//         try
//         {
//             // Validate required fields
//             if (string.IsNullOrWhiteSpace(request.NameEn) || string.IsNullOrWhiteSpace(request.NameAr))
//             {
//                 return Result<CategoryCreatedDto>.Failure("Category name in both English and Arabic is required");
//             }
// 
//             // Check if category with same name already exists
//                 c => c.NameEn.ToLower() == request.NameEn.ToLower() ||
//                      c.NameAr == request.NameAr,
//                 cancellationToken);
// 
//             if (existingCategories != null && existingCategories.Any())
//             {
//                 return Result<CategoryCreatedDto>.Failure("A category with this name already exists");
//             }
// 
//             // Create category
//             var category = new Domain.Entities.Category
//             {
//                 Id = Guid.NewGuid(),
//                 NameEn = request.NameEn.Trim(),
//                 NameAr = request.NameAr.Trim(),
//                 DescriptionEn = request.DescriptionEn?.Trim(),
//                 DescriptionAr = request.DescriptionAr?.Trim(),
//                 IconUrl = request.IconUrl?.Trim(),
//                 IsActive = request.IsActive,
//                 CreatedAt = DateTime.UtcNow,
//                 CreatedBy = request.AdminUserId.ToString()
//             };
// 
//             await _unitOfWork.SaveChangesAsync(cancellationToken);
// 
//             _logger.LogInformation("Category created: {NameEn} ({NameAr}) by admin {AdminId}",
//                 request.NameEn, request.NameAr, request.AdminUserId);
// 
//             var result = new CategoryCreatedDto
//             {
//                 Id = category.Id,
//                 NameEn = category.NameEn,
//                 NameAr = category.NameAr,
//                 Message = "Category created successfully"
//             };
// 
//             return Result<CategoryCreatedDto>.Success(result, "Category created successfully");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error creating category");
//             return Result<CategoryCreatedDto>.Failure("An error occurred while creating the category");
//         }
//     }
// }
// 
