using AutoMapper;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Application.Queries.Bookings;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Bookings;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, Result<PagedResult<BookingDto>>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserBookingsQueryHandler> _logger;

    public GetUserBookingsQueryHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IMapper mapper,
        ILogger<GetUserBookingsQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BookingDto>>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Build query
            var query = await _bookingRepository.FindAsync(
                b => b.CustomerId == request.UserId,
                cancellationToken);

            // Apply status filter
            if (request.Status.HasValue)
            {
                query = query.Where(b => b.Status == request.Status.Value);
            }

            // Order by scheduled date descending
            query = query.OrderByDescending(b => b.ScheduledAt);

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var bookingDtos = _mapper.Map<List<BookingDto>>(items);

            var pagedResult = new PagedResult<BookingDto>
            {
                Items = bookingDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user bookings");
            return Result.Failure<PagedResult<BookingDto>>("An error occurred while retrieving bookings", ex.Message);
        }
    }
}
