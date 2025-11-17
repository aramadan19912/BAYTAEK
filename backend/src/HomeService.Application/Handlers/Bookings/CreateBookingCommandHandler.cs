using AutoMapper;
using HomeService.Application.Commands.Bookings;
using HomeService.Application.Common;
using HomeService.Application.DTOs;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using HomeService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeService.Application.Handlers.Bookings;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private readonly IRepository<HomeService.Domain.Entities.Booking> _bookingRepository;
    private readonly IRepository<HomeService.Domain.Entities.Service> _serviceRepository;
    private readonly IRepository<HomeService.Domain.Entities.Address> _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IRepository<HomeService.Domain.Entities.Booking> bookingRepository,
        IRepository<HomeService.Domain.Entities.Service> serviceRepository,
        IRepository<HomeService.Domain.Entities.Address> addressRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify service exists
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service == null)
            {
                return Result.Failure<BookingDto>("Service not found");
            }

            // Verify address exists and belongs to customer
            var address = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
            if (address == null)
            {
                return Result.Failure<BookingDto>("Address not found");
            }

            if (address.UserId != request.CustomerId)
            {
                return Result.Failure<BookingDto>("Address does not belong to the customer");
            }

            // Calculate VAT based on region
            decimal vatPercentage = address.Region == Region.SaudiArabia ? 15m : 14m;
            decimal vatAmount = service.BasePrice * (vatPercentage / 100);
            decimal totalAmount = service.BasePrice + vatAmount;

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                ServiceId = request.ServiceId,
                AddressId = request.AddressId,
                Status = BookingStatus.Pending,
                ScheduledAt = request.ScheduledAt,
                TotalAmount = totalAmount,
                VatAmount = vatAmount,
                VatPercentage = vatPercentage,
                Currency = service.Currency,
                SpecialInstructions = request.SpecialInstructions,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload booking with related entities for mapping
            var createdBooking = await _bookingRepository.GetByIdAsync(booking.Id, cancellationToken);
            var bookingDto = _mapper.Map<BookingDto>(createdBooking);

            _logger.LogInformation("Booking created successfully: {BookingId}", booking.Id);

            return Result.Success(bookingDto, "Booking created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return Result.Failure<BookingDto>("An error occurred while creating booking", ex.Message);
        }
    }
}
