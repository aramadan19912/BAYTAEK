using HomeService.Application.Common.Models;
using MediatR;

namespace HomeService.Application.Commands.ProviderVerification;

public class RequestVerificationCommand : IRequest<Result<VerificationRequestDto>>
{
    public Guid ProviderId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public List<string> CertificationDocuments { get; set; } = new(); // URLs to uploaded documents
    public string? AdditionalNotes { get; set; }
}

public class VerificationRequestDto
{
    public Guid ProviderId { get; set; }
    public string Status { get; set; } = string.Empty; // "Pending", "UnderReview", "Approved", "Rejected"
    public DateTime RequestedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
