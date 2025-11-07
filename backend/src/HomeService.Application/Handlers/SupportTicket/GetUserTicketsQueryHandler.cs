using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.SupportTicket;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.SupportTicket;

public class GetUserTicketsQueryHandler : IRequestHandler<GetUserTicketsQuery, Result<UserTicketsDto>>
{
    // TODO: Add IRepository<SupportTicket> when SupportTicket entity is created in Domain layer
    // TODO: Add IRepository<TicketMessage> for message counts
    private readonly ILogger<GetUserTicketsQueryHandler> _logger;

    public GetUserTicketsQueryHandler(
        // IRepository<SupportTicket> ticketRepository,
        // IRepository<TicketMessage> ticketMessageRepository,
        // IRepository<User> userRepository,
        ILogger<GetUserTicketsQueryHandler> logger)
    {
        // _ticketRepository = ticketRepository;
        // _ticketMessageRepository = ticketMessageRepository;
        // _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserTicketsDto>> Handle(GetUserTicketsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when SupportTicket entity exists in Domain layer
            /*
            // Get all tickets for the user
            var allTickets = await _ticketRepository.FindAsync(
                t => t.UserId == request.UserId,
                cancellationToken);

            if (allTickets == null)
            {
                return Result<UserTicketsDto>.Success(new UserTicketsDto
                {
                    Tickets = new List<TicketSummaryDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = 0
                }, "No tickets found");
            }

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

            // Calculate summary statistics
            var openCount = allTickets.Count(t => t.Status == TicketStatus.Open);
            var inProgressCount = allTickets.Count(t => t.Status == TicketStatus.InProgress);
            var resolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved);
            var closedCount = allTickets.Count(t => t.Status == TicketStatus.Closed);

            // Order by created date (most recent first)
            tickets = tickets.OrderByDescending(t => t.CreatedAt).ToList();

            // Pagination
            var totalCount = tickets.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var paginatedTickets = tickets
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Get ticket messages for message counts
            var ticketIds = paginatedTickets.Select(t => t.Id).ToList();
            var allMessages = await _ticketMessageRepository.GetAllAsync(cancellationToken);
            var ticketMessages = allMessages.Where(m => ticketIds.Contains(m.TicketId)).ToList();

            // Get assigned users
            var assignedUserIds = paginatedTickets
                .Where(t => t.AssignedToUserId.HasValue)
                .Select(t => t.AssignedToUserId!.Value)
                .Distinct()
                .ToList();
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var assignedUsers = allUsers.Where(u => assignedUserIds.Contains(u.Id)).ToDictionary(u => u.Id);

            // Map to DTOs
            var ticketDtos = paginatedTickets.Select(ticket =>
            {
                var messages = ticketMessages.Where(m => m.TicketId == ticket.Id).ToList();
                var unreadMessages = messages.Count(m => !m.IsReadByUser && !m.IsFromUser);
                var lastMessage = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

                var assignedUser = ticket.AssignedToUserId.HasValue
                    ? assignedUsers.GetValueOrDefault(ticket.AssignedToUserId.Value)
                    : null;

                return new TicketSummaryDto
                {
                    TicketId = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    Subject = ticket.Subject,
                    Category = ticket.Category,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    ResolvedAt = ticket.ResolvedAt,
                    MessagesCount = messages.Count,
                    UnreadMessagesCount = unreadMessages,
                    LastMessagePreview = lastMessage?.Message?.Length > 100
                        ? lastMessage.Message.Substring(0, 100) + "..."
                        : lastMessage?.Message,
                    AssignedToName = assignedUser?.FirstName
                };
            }).ToList();

            var result = new UserTicketsDto
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                OpenTickets = openCount,
                InProgressTickets = inProgressCount,
                ResolvedTickets = resolvedCount,
                ClosedTickets = closedCount
            };

            return Result<UserTicketsDto>.Success(result, "User tickets retrieved successfully");
            */

            // Temporary placeholder response
            _logger.LogWarning("GetUserTicketsQuery called but SupportTicket entity not yet implemented");

            var emptyResult = new UserTicketsDto
            {
                Tickets = new List<TicketSummaryDto>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = 0,
                OpenTickets = 0,
                InProgressTickets = 0,
                ResolvedTickets = 0,
                ClosedTickets = 0
            };

            return Result<UserTicketsDto>.Success(emptyResult,
                "SupportTicket system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for user {UserId}", request.UserId);
            return Result<UserTicketsDto>.Failure("An error occurred while retrieving tickets");
        }
    }
}
