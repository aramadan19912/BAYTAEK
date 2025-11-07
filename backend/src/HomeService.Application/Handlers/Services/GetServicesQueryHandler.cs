using AutoMapper;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Application.Queries.Services;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Services;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, Result<PagedResult<ServiceDto>>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetServicesQueryHandler> _logger;

    public GetServicesQueryHandler(
        IRepository<Service> serviceRepository,
        IMapper mapper,
        ILogger<GetServicesQueryHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Build query
            var query = await _serviceRepository.FindAsync(s => s.IsActive, cancellationToken);

            // Apply filters
            if (request.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == request.CategoryId.Value);
            }

            if (request.Region.HasValue)
            {
                query = query.Where(s => s.AvailableRegions.Contains(request.Region.Value));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.NameEn.ToLower().Contains(searchLower) ||
                    s.NameAr.Contains(searchLower) ||
                    s.DescriptionEn.ToLower().Contains(searchLower) ||
                    s.DescriptionAr.Contains(searchLower));
            }

            if (request.IsFeatured.HasValue)
            {
                query = query.Where(s => s.IsFeatured == request.IsFeatured.Value);
            }

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var serviceDtos = _mapper.Map<List<ServiceDto>>(items);

            var pagedResult = new PagedResult<ServiceDto>
            {
                Items = serviceDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services");
            return Result.Failure<PagedResult<ServiceDto>>("An error occurred while retrieving services", ex.Message);
        }
    }
}
