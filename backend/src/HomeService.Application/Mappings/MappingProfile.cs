using AutoMapper;
using HomeService.Application.DTOs;
using HomeService.Domain.Entities;

namespace HomeService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage.ToString()))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region.ToString()));

        CreateMap<User, Commands.Users.UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage.ToString()))
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region.ToString()));

        // Service mappings
        CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionEn))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()));

        // Booking mappings
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.NameEn))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src =>
                src.Provider != null ? src.Provider.BusinessName : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region.ToString()));

        // ServiceCategory mappings
        CreateMap<ServiceCategory, ServiceCategoryDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DescriptionEn));

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                $"{src.Customer.FirstName} {src.Customer.LastName}"));

        // Payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()));
    }
}

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string[] ImageUrls { get; set; } = Array.Empty<string>();
    public string? ProviderResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
