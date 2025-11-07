using HomeService.Application.Common;
using HomeService.Application.DTOs.Admin;
using HomeService.Application.Queries.Admin;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<PagedResult<AdminUserListDto>>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly ILogger<GetAdminUsersQueryHandler> _logger;

    public GetAdminUsersQueryHandler(
        IRepository<User> userRepository,
        IRepository<Booking> bookingRepository,
        IRepository<Payment> paymentRepository,
        ILogger<GetAdminUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AdminUserListDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = await _userRepository.GetAllAsync(cancellationToken);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.PhoneNumber.Contains(searchLower));
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.Role == request.Role.Value);
            }

            if (request.Region.HasValue)
            {
                query = query.Where(u => u.Region == request.Region.Value);
            }

            if (request.IsVerified.HasValue)
            {
                query = query.Where(u => u.IsEmailVerified == request.IsVerified.Value);
            }

            if (request.RegisteredAfter.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= request.RegisteredAfter.Value);
            }

            if (request.RegisteredBefore.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= request.RegisteredBefore.Value);
            }

            // Get all bookings and payments for statistics
            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);

            // Order by creation date descending
            query = query.OrderByDescending(u => u.CreatedAt);

            var totalCount = query.Count();

            // Apply pagination
            var users = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var userDtos = users.Select(u =>
            {
                var userBookings = allBookings.Where(b => b.CustomerId == u.Id).ToList();
                var completedBookings = userBookings.Where(b => b.Status == Domain.Enums.BookingStatus.Completed);
                var totalSpent = allPayments
                    .Where(p => completedBookings.Any(b => b.Id == p.BookingId))
                    .Sum(p => p.Amount);

                return new AdminUserListDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                    Region = u.Region.ToString(),
                    IsEmailVerified = u.IsEmailVerified,
                    IsPhoneVerified = u.IsPhoneVerified,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = !u.IsDeleted,
                    TotalBookings = userBookings.Count,
                    TotalSpent = totalSpent
                };
            }).ToList();

            var pagedResult = new PagedResult<AdminUserListDto>
            {
                Items = userDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin users");
            return Result.Failure<PagedResult<AdminUserListDto>>("Error retrieving users", ex.Message);
        }
    }
}
