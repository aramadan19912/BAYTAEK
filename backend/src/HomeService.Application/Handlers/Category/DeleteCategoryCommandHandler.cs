using HomeService.Application.Commands.Category;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Category;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Category> _categoryRepository;
    private readonly IRepository<Domain.Entities.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        IRepository<Domain.Entities.Category> categoryRepository,
        IRepository<Domain.Entities.Service> serviceRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get category
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<bool>.Failure("Category not found");
            }

            // Check if category has any services
            var servicesInCategory = await _serviceRepository.FindAsync(
                s => s.CategoryId == request.CategoryId,
                cancellationToken);

            if (servicesInCategory != null && servicesInCategory.Any())
            {
                var serviceCount = servicesInCategory.Count();
                return Result<bool>.Failure(
                    $"Cannot delete category. It has {serviceCount} service(s) associated with it. " +
                    "Please reassign or delete the services first.");
            }

            // Delete category
            await _categoryRepository.DeleteAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} ({NameEn}) deleted by admin {AdminId}",
                request.CategoryId, category.NameEn, request.AdminUserId);

            return Result<bool>.Success(true, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", request.CategoryId);
            return Result<bool>.Failure("An error occurred while deleting the category");
        }
    }
}
