using HomeService.Application.Common;
using HomeService.Domain.Entities;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Features.Providers;

public record UpdateServicePortfolioCommand(
    Guid ProviderId,
    string Description,
    List<string> PortfolioImages,
    List<string> CertificationDocuments
) : IRequest<Result<ServicePortfolioDto>>;

public class UpdateServicePortfolioCommandHandler
    : IRequestHandler<UpdateServicePortfolioCommand, Result<ServicePortfolioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateServicePortfolioCommandHandler> _logger;

    public UpdateServicePortfolioCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateServicePortfolioCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServicePortfolioDto>> Handle(
        UpdateServicePortfolioCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get provider
            var provider = await _unitOfWork.Repository<ServiceProvider>()
                .GetByIdAsync(request.ProviderId, cancellationToken);

            if (provider == null)
                return Result.Failure<ServicePortfolioDto>("Provider not found");

            // Update portfolio
            provider.Description = request.Description;
            provider.Portfolio = string.Join(",", request.PortfolioImages);
            provider.CertificationDocuments = string.Join(",", request.CertificationDocuments);
            provider.UpdatedAt = DateTime.UtcNow;
            provider.UpdatedBy = request.ProviderId.ToString();

            _unitOfWork.Repository<ServiceProvider>().Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Portfolio updated for provider {ProviderId}", request.ProviderId);

            var dto = new ServicePortfolioDto
            {
                ProviderId = provider.Id,
                Description = provider.Description,
                PortfolioImages = request.PortfolioImages,
                CertificationDocuments = request.CertificationDocuments
            };

            return Result.Success(dto, "Portfolio updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio for provider {ProviderId}",
                request.ProviderId);
            return Result.Failure<ServicePortfolioDto>(
                "An error occurred while updating portfolio",
                ex.Message);
        }
    }
}

public class ServicePortfolioDto
{
    public Guid ProviderId { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> PortfolioImages { get; set; } = new();
    public List<string> CertificationDocuments { get; set; } = new();
}
