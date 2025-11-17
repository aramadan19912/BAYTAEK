using HomeService.Application.Commands.ProviderVerification;
using HomeService.Domain.Interfaces;
using HomeService.Application.Common;
using HomeService.Domain.Interfaces;
using HomeService.Application.Interfaces;
using HomeService.Domain.Interfaces;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using HomeService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using HomeService.Domain.Interfaces;

namespace HomeService.Application.Handlers.ProviderVerification;

public class RequestVerificationCommandHandler : IRequestHandler<RequestVerificationCommand, Result<VerificationRequestDto>>
{
    private readonly IRepository<ServiceProvider> _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<RequestVerificationCommandHandler> _logger;

    public RequestVerificationCommandHandler(
        IRepository<ServiceProvider> providerRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<RequestVerificationCommandHandler> logger)
    {
        _providerRepository = providerRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<VerificationRequestDto>> Handle(RequestVerificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
            if (provider == null)
            {
                return Result<VerificationRequestDto>.Failure("Provider not found");
            }

            // Check if already verified
            if (provider.IsVerified)
            {
                return Result<VerificationRequestDto>.Failure("Provider is already verified");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            {
                return Result<VerificationRequestDto>.Failure("License number is required");
            }

            if (!request.CertificationDocuments.Any())
            {
                return Result<VerificationRequestDto>.Failure("At least one certification document is required");
            }

            // Update provider with verification documents
            provider.LicenseNumber = request.LicenseNumber.Trim();
            provider.CertificationDocuments = request.CertificationDocuments.ToArray();
            provider.VerificationStatus = "Pending"; // Add this field to ServiceProvider entity
            provider.VerificationRequestedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.ProviderId.ToString();

            await _providerRepository.UpdateAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Verification requested by provider {ProviderId}", request.ProviderId);

            // TODO: Send email notification to admin
            /*
            try
            {
                await _emailService.SendProviderVerificationRequestEmailAsync(
                    "admin@homeservice.com",
                    provider.BusinessName,
                    request.ProviderId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification request email");
            }
            */

            var result = new VerificationRequestDto
            {
                ProviderId = provider.Id,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                Message = "Verification request submitted successfully. You will be notified once your documents are reviewed."
            };

            return Result<VerificationRequestDto>.Success(result,
                "Verification request submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting verification for provider {ProviderId}", request.ProviderId);
            return Result<VerificationRequestDto>.Failure("An error occurred while submitting verification request");
        }
    }
}
