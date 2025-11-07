using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Application.Queries.SupportTicket;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.SupportTicket;

public class GetTicketDetailQueryHandler : IRequestHandler<GetTicketDetailQuery, Result<TicketDetailDto>>
{
    // TODO: Add IRepository<SupportTicket> when SupportTicket entity is created in Domain layer
    // TODO: Add IRepository<TicketMessage> for messages
    // TODO: Add IRepository<Booking> for booking info
    private readonly ILogger<GetTicketDetailQueryHandler> _logger;

    public GetTicketDetailQueryHandler(
        // IRepository<SupportTicket> ticketRepository,
        // IRepository<TicketMessage> ticketMessageRepository,
        // IRepository<User> userRepository,
        // IRepository<Booking> bookingRepository,
        ILogger<GetTicketDetailQueryHandler> logger)
    {
        // _ticketRepository = ticketRepository;
        // _ticketMessageRepository = ticketMessageRepository;
        // _userRepository = userRepository;
        // _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<TicketDetailDto>> Handle(GetTicketDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when SupportTicket entity exists in Domain layer
            /*
            // Get ticket
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
            {
                return Result<TicketDetailDto>.Failure("Ticket not found");
            }

            // Verify user owns this ticket (or is admin)
            if (ticket.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to access ticket {TicketId} they don't own",
                    request.UserId, request.TicketId);
                return Result<TicketDetailDto>.Failure("You are not authorized to view this ticket");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(ticket.UserId, cancellationToken);

            // Get assigned user
            User? assignedUser = null;
            if (ticket.AssignedToUserId.HasValue)
            {
                assignedUser = await _userRepository.GetByIdAsync(ticket.AssignedToUserId.Value, cancellationToken);
            }

            // Get booking if exists
            Booking? booking = null;
            if (ticket.BookingId.HasValue)
            {
                booking = await _bookingRepository.GetByIdAsync(ticket.BookingId.Value, cancellationToken);
            }

            // Get all messages for this ticket
            var allMessages = await _ticketMessageRepository.FindAsync(
                m => m.TicketId == ticket.Id,
                cancellationToken);
            var messages = allMessages?.OrderBy(m => m.CreatedAt).ToList() ?? new List<TicketMessage>();

            // Get message senders
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var senders = allUsers.Where(u => senderIds.Contains(u.Id)).ToDictionary(u => u.Id);

            // Mark unread admin messages as read by user
            var unreadMessages = messages.Where(m => !m.IsFromUser && !m.IsReadByUser).ToList();
            foreach (var message in unreadMessages)
            {
                message.IsReadByUser = true;
                message.ReadByUserAt = DateTime.UtcNow;
                await _ticketMessageRepository.UpdateAsync(message, cancellationToken);
            }

            if (unreadMessages.Any())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Map messages to DTOs
            var messageDtos = messages.Select(m =>
            {
                var sender = senders.GetValueOrDefault(m.SenderId);
                return new TicketMessageDto
                {
                    MessageId = m.Id,
                    Message = m.Message,
                    IsFromUser = m.IsFromUser,
                    SenderId = m.SenderId,
                    SenderName = sender?.FirstName ?? "Unknown",
                    Attachments = m.Attachments?.ToList() ?? new List<string>(),
                    IsReadByUser = m.IsReadByUser,
                    IsReadByAdmin = m.IsReadByAdmin,
                    CreatedAt = m.CreatedAt
                };
            }).ToList();

            var result = new TicketDetailDto
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,
                BookingId = ticket.BookingId,
                BookingNumber = booking?.BookingNumber,
                UserId = ticket.UserId,
                UserName = user?.FirstName ?? "Unknown User",
                UserEmail = user?.Email ?? "",
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToName = assignedUser?.FirstName,
                AssignedAt = ticket.AssignedAt,
                Attachments = ticket.Attachments?.ToList() ?? new List<string>(),
                Messages = messageDtos,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                ResolutionNotes = ticket.ResolutionNotes
            };

            return Result<TicketDetailDto>.Success(result, "Ticket details retrieved successfully");
            */

            // Temporary placeholder response
            _logger.LogWarning("GetTicketDetailQuery called but SupportTicket entity not yet implemented");

            return Result<TicketDetailDto>.Failure("SupportTicket system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ticket details for ticket {TicketId}", request.TicketId);
            return Result<TicketDetailDto>.Failure("An error occurred while retrieving ticket details");
        }
    }
}
