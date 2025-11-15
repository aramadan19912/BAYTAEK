using HomeService.Application.Commands.SupportTicket;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.SupportTicket;

public class AddTicketMessageCommandHandler : IRequestHandler<AddTicketMessageCommand, Result<TicketMessageAddedDto>>
{
    // TODO: Add IRepository<SupportTicket> when SupportTicket entity is created in Domain layer
    // TODO: Add IRepository<TicketMessage> for messages
    // TODO: Add IEmailService and IPushNotificationService for notifications
    private readonly ILogger<AddTicketMessageCommandHandler> _logger;

    public AddTicketMessageCommandHandler(
        // IRepository<SupportTicket> ticketRepository,
        // IRepository<TicketMessage> ticketMessageRepository,
        // IRepository<User> userRepository,
        // IUnitOfWork unitOfWork,
        // IEmailService emailService,
        // IPushNotificationService pushNotificationService,
        ILogger<AddTicketMessageCommandHandler> logger)
    {
        // _ticketRepository = ticketRepository;
        // _ticketMessageRepository = ticketMessageRepository;
        // _userRepository = userRepository;
        // _unitOfWork = unitOfWork;
        // _emailService = emailService;
        // _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Result<TicketMessageAddedDto>> Handle(AddTicketMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implement when SupportTicket entity exists in Domain layer
            /*
            // Get ticket
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
            {
                return Result<TicketMessageAddedDto>.Failure("Ticket not found");
            }

            // Verify user owns this ticket or is admin
            if (ticket.UserId != request.UserId && !request.IsFromUser)
            {
                // This is from admin, allow
            }
            else if (ticket.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to add message to ticket {TicketId} they don't own",
                    request.UserId, request.TicketId);
                return Result<TicketMessageAddedDto>.Failure("You are not authorized to add messages to this ticket");
            }

            // Validate message
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Result<TicketMessageAddedDto>.Failure("Message cannot be empty");
            }

            // If ticket is closed, reopen it
            if (ticket.Status == TicketStatus.Closed)
            {
                ticket.Status = TicketStatus.Reopened;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            }

            // Create message
            var message = new Domain.Entities.TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = request.TicketId,
                SenderId = request.UserId,
                Message = request.Message.Trim(),
                IsFromUser = request.IsFromUser,
                Attachments = request.Attachments?.ToArray(),
                IsReadByUser = request.IsFromUser, // Auto-read if from user
                IsReadByAdmin = !request.IsFromUser, // Auto-read if from admin
                CreatedAt = DateTime.UtcNow
            };

            await _ticketMessageRepository.AddAsync(message, cancellationToken);

            // Update ticket's UpdatedAt
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notifications
            if (request.IsFromUser)
            {
                // Notify admin team of new user message
                await _emailService.SendTicketMessageNotificationToAdminAsync(
                    ticket.TicketNumber,
                    ticket.Subject,
                    request.Message,
                    cancellationToken);
            }
            else
            {
                // Notify user of admin response
                var user = await _userRepository.GetByIdAsync(ticket.UserId, cancellationToken);
                if (user != null)
                {
                    await _emailService.SendTicketResponseEmailAsync(
                        user.Email,
                        user.FirstName,
                        ticket.TicketNumber,
                        ticket.Subject,
                        request.Message,
                        user.PreferredLanguage,
                        cancellationToken);

                    await _pushNotificationService.SendNotificationAsync(
                        ticket.UserId,
                        "Support Ticket Response",
                        $"You have a new response on ticket {ticket.TicketNumber}",
                        new Dictionary<string, string>
                        {
                            { "type", "ticket_response" },
                            { "ticketId", ticket.Id.ToString() },
                            { "ticketNumber", ticket.TicketNumber }
                        },
                        cancellationToken);
                }
            }

            _logger.LogInformation("Message added to ticket {TicketNumber} by {IsFromUser}",
                ticket.TicketNumber, request.IsFromUser ? "user" : "admin");

            return Result<TicketMessageAddedDto>.Success(new TicketMessageAddedDto
            {
                MessageId = message.Id,
                CreatedAt = message.CreatedAt,
                Message = message.Message
            }, "Message added successfully");
            */

            // Temporary placeholder response
            _logger.LogWarning("AddTicketMessageCommand called but SupportTicket entity not yet implemented");

            var placeholderResult = new TicketMessageAddedDto
            {
                MessageId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Message = request.Message
            };

            return Result<TicketMessageAddedDto>.Success(placeholderResult,
                "SupportTicket system pending domain entity implementation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message to ticket {TicketId}", request.TicketId);
            return Result<TicketMessageAddedDto>.Failure("An error occurred while adding the message");
        }
    }
}
