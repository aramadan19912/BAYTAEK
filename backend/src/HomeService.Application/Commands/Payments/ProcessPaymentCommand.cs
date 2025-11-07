using HomeService.Application.Common;
using HomeService.Application.Mappings;
using HomeService.Domain.Enums;
using MediatR;

namespace HomeService.Application.Commands.Payments;

public record ProcessPaymentCommand : IRequest<Result<PaymentDto>>
{
    public Guid BookingId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? PaymentToken { get; init; }
}
