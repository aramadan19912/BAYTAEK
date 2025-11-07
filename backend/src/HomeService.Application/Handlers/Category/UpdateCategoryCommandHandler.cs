using HomeService.Application.Commands.Category;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Category;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<bool>>
{
    private readonly IRepository<Domain.Entities.Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IRepository<Domain.Entities.Category> categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get category
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<bool>.Failure("Category not found");
            }

            // Track if any changes were made
            var hasChanges = false;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.NameEn) && request.NameEn != category.NameEn)
            {
                // Check for duplicate name
                var existingWithName = await _categoryRepository.FindAsync(
                    c => c.NameEn.ToLower() == request.NameEn.ToLower() && c.Id != request.CategoryId,
                    cancellationToken);

                if (existingWithName != null && existingWithName.Any())
                {
                    return Result<bool>.Failure("A category with this English name already exists");
                }

                category.NameEn = request.NameEn.Trim();
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(request.NameAr) && request.NameAr != category.NameAr)
            {
                // Check for duplicate name
                var existingWithName = await _categoryRepository.FindAsync(
                    c => c.NameAr == request.NameAr && c.Id != request.CategoryId,
                    cancellationToken);

                if (existingWithName != null && existingWithName.Any())
                {
                    return Result<bool>.Failure("A category with this Arabic name already exists");
                }

                category.NameAr = request.NameAr.Trim();
                hasChanges = true;
            }

            if (request.DescriptionEn != null && request.DescriptionEn != category.DescriptionEn)
            {
                category.DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim();
                hasChanges = true;
            }

            if (request.DescriptionAr != null && request.DescriptionAr != category.DescriptionAr)
            {
                category.DescriptionAr = string.IsNullOrWhiteSpace(request.DescriptionAr) ? null : request.DescriptionAr.Trim();
                hasChanges = true;
            }

            if (request.IconUrl != null && request.IconUrl != category.IconUrl)
            {
                category.IconUrl = string.IsNullOrWhiteSpace(request.IconUrl) ? null : request.IconUrl.Trim();
                hasChanges = true;
            }

            if (request.IsActive.HasValue && request.IsActive.Value != category.IsActive)
            {
                category.IsActive = request.IsActive.Value;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                return Result<bool>.Success(true, "No changes detected");
            }

            // Update audit fields
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = request.AdminUserId.ToString();

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category {CategoryId} updated by admin {AdminId}",
                request.CategoryId, request.AdminUserId);

            return Result<bool>.Success(true, "Category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", request.CategoryId);
            return Result<bool>.Failure("An error occurred while updating the category");
        }
    }
}
