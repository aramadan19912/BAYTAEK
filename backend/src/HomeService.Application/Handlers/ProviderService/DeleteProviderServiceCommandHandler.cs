using HomeService.Application.Commands.ProviderService;
using HomeService.Application.Common.Models;
using HomeService.Application.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.ProviderService;

public class DeleteProviderServiceCommandHandler : IRequestHandler<DeleteProviderServiceCommand, Result<bool>>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProviderServiceCommandHandler> _logger;

    public DeleteProviderServiceCommandHandler(
        IRepository<Service> serviceRepository,
        IRepository<Booking> bookingRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProviderServiceCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProviderServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get service
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result<bool>.Failure("Service not found");
            }

            // Verify provider owns this service
            if (service.ProviderId != request.ProviderId)
            {
                _logger.LogWarning("Provider {ProviderId} attempted to delete service {ServiceId} they don't own",
                    request.ProviderId, request.ServiceId);
                return Result<bool>.Failure("You are not authorized to delete this service");
            }

            // Check for active bookings
            var activeBookings = await _bookingRepository.FindAsync(
                b => b.ServiceId == request.ServiceId &&
                     (b.Status == BookingStatus.Pending ||
                      b.Status == BookingStatus.Confirmed ||
                      b.Status == BookingStatus.InProgress),
                cancellationToken);

            if (activeBookings != null && activeBookings.Any())
            {
                return Result<bool>.Failure(
                    $"Cannot delete service with active bookings. Please complete or cancel {activeBookings.Count()} active booking(s) first.");
            }

            // Delete service
            await _serviceRepository.DeleteAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service {ServiceId} ({NameEn}) deleted by provider {ProviderId}",
                request.ServiceId, service.NameEn, request.ProviderId);

            return Result<bool>.Success(true, "Service deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service {ServiceId}", request.ServiceId);
            return Result<bool>.Failure("An error occurred while deleting the service");
        }
    }
}
