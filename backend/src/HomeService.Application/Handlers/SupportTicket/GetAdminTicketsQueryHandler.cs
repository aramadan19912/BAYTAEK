using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Application.Queries.SupportTicket;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.SupportTicket;

public class GetAdminTicketsQueryHandler : IRequestHandler<GetAdminTicketsQuery, Result<AdminTicketsDto>>
{
    // TODO: Add IRepository<SupportTicket> when SupportTicket entity is created in Domain layer
    // TODO: Add IRepository<TicketMessage> for message counts
    // TODO: Add IRepository<HomeService.Domain.Entities.User> for user info
    // TODO: Add IRepository<HomeService.Domain.Entities.Booking> for booking info
    private readonly ILogger<GetAdminTicketsQueryHandler> _logger;

    public GetAdminTicketsQueryHandler(
        // IRepository<SupportTicket> ticketRepository,
        // IRepository<TicketMessage> ticketMessageRepository,
        // IRepository<HomeService.Domain.Entities.User> userRepository,
        // IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        ILogger<GetAdminTicketsQueryHandler> logger)
    {
        // _ticketRepository = ticketRepository;
        // _ticketMessageRepository = ticketMessageRepository;
        // _userRepository = userRepository;
        // _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<AdminTicketsDto>> Handle(GetAdminTicketsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when SupportTicket entity exists in Domain layer
            /*
            // Get all tickets
            var allTickets = await _ticketRepository.GetAllAsync(cancellationToken);
            var tickets = allTickets.ToList();

            // Apply filters
            if (request.Status.HasValue)
            {
                tickets = tickets.Where(t => t.Status == request.Status.Value).ToList();
            }

            if (request.Category.HasValue)
            {
                tickets = tickets.Where(t => t.Category == request.Category.Value).ToList();
            }

            if (request.Priority.HasValue)
            {
                tickets = tickets.Where(t => t.Priority == request.Priority.Value).ToList();
            }

            if (request.UserId.HasValue)
            {
                tickets = tickets.Where(t => t.UserId == request.UserId.Value).ToList();
            }

            if (request.AssignedToUserId.HasValue)
            {
                tickets = tickets.Where(t => t.AssignedToUserId == request.AssignedToUserId.Value).ToList();
            }

            // Search term (search in ticket number, subject, user name, user email)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                var allUsers = await _userRepository.GetAllAsync(cancellationToken);
                var matchingUserIds = allUsers
                    .Where(u => u.FirstName.ToLower().Contains(searchTerm) ||
                                u.Email.ToLower().Contains(searchTerm))
                    .Select(u => u.Id)
                    .ToHashSet();

                tickets = tickets.Where(t =>
                    t.TicketNumber.ToLower().Contains(searchTerm) ||
                    t.Subject.ToLower().Contains(searchTerm) ||
                    matchingUserIds.Contains(t.UserId)).ToList();
            }

            // Calculate summary statistics
            var openCount = allTickets.Count(t => t.Status == TicketStatus.Open);
            var inProgressCount = allTickets.Count(t => t.Status == TicketStatus.InProgress);
            var resolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved);
            var closedCount = allTickets.Count(t => t.Status == TicketStatus.Closed);
            var unassignedCount = allTickets.Count(t => !t.AssignedToUserId.HasValue);
            var highPriorityCount = allTickets.Count(t => t.Priority == TicketPriority.High);
            var urgentCount = allTickets.Count(t => t.Priority == TicketPriority.Urgent);

            // Order by priority (urgent first), then by created date (most recent first)
            tickets = tickets
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            // Pagination
            var totalCount = tickets.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var paginatedTickets = tickets
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Load related data efficiently
            var ticketIds = paginatedTickets.Select(t => t.Id).ToList();
            var userIds = paginatedTickets.Select(t => t.UserId).Distinct().ToList();
            var assignedUserIds = paginatedTickets
                .Where(t => t.AssignedToUserId.HasValue)
                .Select(t => t.AssignedToUserId!.Value)
                .Distinct()
                .ToList();
            var bookingIds = paginatedTickets
                .Where(t => t.BookingId.HasValue)
                .Select(t => t.BookingId!.Value)
                .Distinct()
                .ToList();

            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var usersDict = allUsers.Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);
            var assignedUsersDict = allUsers.Where(u => assignedUserIds.Contains(u.Id)).ToDictionary(u => u.Id);

            var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
            var bookingsDict = allBookings.Where(b => bookingIds.Contains(b.Id)).ToDictionary(b => b.Id);

            var allMessages = await _ticketMessageRepository.GetAllAsync(cancellationToken);
            var ticketMessages = allMessages.Where(m => ticketIds.Contains(m.TicketId)).ToList();

            // Map to DTOs
            var ticketDtos = paginatedTickets.Select(ticket =>
            {
                var user = usersDict.GetValueOrDefault(ticket.UserId);
                var assignedUser = ticket.AssignedToUserId.HasValue
                    ? assignedUsersDict.GetValueOrDefault(ticket.AssignedToUserId.Value)
                    : null;
                var booking = ticket.BookingId.HasValue
                    ? bookingsDict.GetValueOrDefault(ticket.BookingId.Value)
                    : null;

                var messages = ticketMessages.Where(m => m.TicketId == ticket.Id).ToList();
                var unreadByAdminCount = messages.Count(m => m.IsFromUser && !m.IsReadByAdmin);
                var lastMessage = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

                return new AdminTicketSummaryDto
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Subject = ticket.Subject,
                    Category = ticket.Category,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    UserId = ticket.UserId,
                    UserName = user?.FirstName ?? "Unknown User",
                    UserEmail = user?.Email ?? "",
                    UserPhone = user?.PhoneNumber,
                    AssignedToUserId = ticket.AssignedToUserId,
                    AssignedToName = assignedUser?.FirstName,
                    AssignedAt = ticket.AssignedAt,
                    BookingId = ticket.BookingId,
                    BookingNumber = booking?.BookingNumber,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    ResolvedAt = ticket.ResolvedAt,
                    MessagesCount = messages.Count,
                    UnreadByAdminCount = unreadByAdminCount,
                    LastMessagePreview = lastMessage?.Message?.Length > 100
                        ? lastMessage.Message.Substring(0, 100) + "..."
                        : lastMessage?.Message,
                    LastMessageAt = lastMessage?.CreatedAt
                };
            }).ToList();

            var result = new AdminTicketsDto
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                OpenTickets = openCount,
                InProgressTickets = inProgressCount,
                ResolvedTickets = resolvedCount,
                ClosedTickets = closedCount,
                UnassignedTickets = unassignedCount,
                HighPriorityTickets = highPriorityCount,
                UrgentTickets = urgentCount
            };

            return Result<AdminTicketsDto>.Success(result, "Admin tickets retrieved successfully");
            */

            // Temporary placeholder response
            _logger.LogWarning("GetAdminTicketsQuery called but SupportTicket entity not yet implemented");

            var emptyResult = new AdminTicketsDto
            {
                Tickets = new List<AdminTicketSummaryDto>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = 0,
                OpenTickets = 0,
                InProgressTickets = 0,
                ResolvedTickets = 0,
                ClosedTickets = 0,
                UnassignedTickets = 0,
                HighPriorityTickets = 0,
                UrgentTickets = 0
            };

            return Result<AdminTicketsDto>.Success(emptyResult,
                "SupportTicket system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin tickets");
            return Result<AdminTicketsDto>.Failure("An error occurred while retrieving tickets");
        }
    }
}
