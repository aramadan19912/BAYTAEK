using AutoMapper;
using HomeService.Application.Common;
using HomeService.Application.Mappings;
using HomeService.Application.Queries.Services;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;

namespace HomeService.Application.Handlers.Services;

public class GetServiceCategoriesQueryHandler : IRequestHandler<GetServiceCategoriesQuery, Result<List<ServiceCategoryDto>>>
{
    private readonly IRepository<ServiceCategory> _categoryRepository;
    private readonly IMapper _mapper;

    public GetServiceCategoriesQueryHandler(
        IRepository<ServiceCategory> categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<ServiceCategoryDto>>> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _categoryRepository.GetAllAsync(cancellationToken);

            if (!request.IncludeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            if (request.ParentCategoryId.HasValue)
            {
                query = query.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
            }
            else
            {
                // Get root categories only
                query = query.Where(c => c.ParentCategoryId == null);
            }

            var categories = query.OrderBy(c => c.DisplayOrder).ToList();
            var categoryDtos = _mapper.Map<List<ServiceCategoryDto>>(categories);

            return Result.Success(categoryDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ServiceCategoryDto>>("An error occurred while retrieving categories", ex.Message);
        }
    }
}
