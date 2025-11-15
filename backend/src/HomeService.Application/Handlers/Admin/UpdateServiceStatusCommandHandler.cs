using HomeService.Application.Commands.Admin;
using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Admin;

public class UpdateServiceStatusCommandHandler : IRequestHandler<UpdateServiceStatusCommand, Result>
{
    private readonly IRepository<Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateServiceStatusCommandHandler> _logger;

    public UpdateServiceStatusCommandHandler(
        IRepository<Service> serviceRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateServiceStatusCommandHandler> logger)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateServiceStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result.Failure("Service not found");
            }

            service.IsDeleted = !request.IsActive;
            service.UpdatedAt = DateTime.UtcNow;
            service.UpdatedBy = request.AdminUserId.ToString();

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var action = request.IsActive ? "activated" : "deactivated";
            _logger.LogInformation("Service {ServiceId} ({ServiceName}) has been {Action} by admin {AdminId}. Reason: {Reason}",
                request.ServiceId, service.Name, action, request.AdminUserId, request.Reason);

            return Result.Success($"Service {action} successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service status");
            return Result.Failure("Error updating service status", ex.Message);
        }
    }
}
