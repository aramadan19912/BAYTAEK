using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HomeService.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database already seeded
        }

        // Seed Users
        var users = await SeedUsersAsync(context);

        // Seed Service Categories
        var categories = await SeedServiceCategoriesAsync(context);

        // Seed Service Providers
        var providers = await SeedServiceProvidersAsync(context, users);

        // Seed Services
        var services = await SeedServicesAsync(context, categories, providers);

        // Seed Addresses
        var addresses = await SeedAddressesAsync(context, users);

        // Seed Bookings
        var bookings = await SeedBookingsAsync(context, users, services, addresses);

        // Seed Reviews
        await SeedReviewsAsync(context, bookings);

        // Seed System Configuration
        await SeedSystemConfigurationAsync(context);

        // Seed Promo Codes
        await SeedPromoCodesAsync(context, services, categories);

        await context.SaveChangesAsync();
    }

    private static async Task<List<User>> SeedUsersAsync(ApplicationDbContext context)
    {
        var users = new List<User>
        {
            // Admin User
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@baytaek.com",
                PhoneNumber = "+966500000001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                PreferredLanguage = Language.Arabic,
                Region = Region.SaudiArabia,
                ProfileImageUrl = "https://ui-avatars.com/api/?name=Admin+User"
            },
            // Customer 1
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ahmed",
                LastName = "AlSaeed",
                Email = "ahmed@example.com",
                PhoneNumber = "+966500000002",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role = UserRole.Customer,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                PreferredLanguage = Language.Arabic,
                Region = Region.SaudiArabia,
                ProfileImageUrl = "https://ui-avatars.com/api/?name=Ahmed+AlSaeed"
            },
            // Customer 2
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Fatima",
                LastName = "AlMutairi",
                Email = "fatima@example.com",
                PhoneNumber = "+966500000003",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role = UserRole.Customer,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                PreferredLanguage = Language.Arabic,
                Region = Region.SaudiArabia,
                ProfileImageUrl = "https://ui-avatars.com/api/?name=Fatima+AlMutairi"
            },
            // Provider 1
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Mohammed",
                LastName = "Hassan",
                Email = "mohammed.plumber@example.com",
                PhoneNumber = "+966500000004",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Provider@123"),
                Role = UserRole.ServiceProvider,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                PreferredLanguage = Language.Arabic,
                Region = Region.SaudiArabia,
                ProfileImageUrl = "https://ui-avatars.com/api/?name=Mohammed+Hassan"
            },
            // Provider 2
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Khalid",
                LastName = "AlGhamdi",
                Email = "khalid.electrician@example.com",
                PhoneNumber = "+966500000005",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Provider@123"),
                Role = UserRole.ServiceProvider,
                IsEmailVerified = true,
                IsPhoneVerified = true,
                PreferredLanguage = Language.Arabic,
                Region = Region.SaudiArabia,
                ProfileImageUrl = "https://ui-avatars.com/api/?name=Khalid+AlGhamdi"
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        return users;
    }

    private static async Task<List<ServiceCategory>> SeedServiceCategoriesAsync(ApplicationDbContext context)
    {
        var categories = new List<ServiceCategory>
        {
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "السباكة",
                NameEn = "Plumbing",
                DescriptionAr = "خدمات السباكة والصرف الصحي",
                DescriptionEn = "Plumbing and drainage services",
                IconUrl = "plumbing-icon.svg",
                DisplayOrder = 1,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "الكهرباء",
                NameEn = "Electrical",
                DescriptionAr = "خدمات الكهرباء والصيانة",
                DescriptionEn = "Electrical and maintenance services",
                IconUrl = "electrical-icon.svg",
                DisplayOrder = 2,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "التنظيف",
                NameEn = "Cleaning",
                DescriptionAr = "خدمات التنظيف المنزلي والتجاري",
                DescriptionEn = "Home and commercial cleaning services",
                IconUrl = "cleaning-icon.svg",
                DisplayOrder = 3,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "النجارة",
                NameEn = "Carpentry",
                DescriptionAr = "خدمات النجارة والأثاث",
                DescriptionEn = "Carpentry and furniture services",
                IconUrl = "carpentry-icon.svg",
                DisplayOrder = 4,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "صيانة التكييف",
                NameEn = "AC Maintenance",
                DescriptionAr = "خدمات صيانة وتركيب التكييف",
                DescriptionEn = "AC installation and maintenance services",
                IconUrl = "ac-icon.svg",
                DisplayOrder = 5,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = Guid.NewGuid(),
                NameAr = "الدهانات",
                NameEn = "Painting",
                DescriptionAr = "خدمات الدهانات والديكور",
                DescriptionEn = "Painting and decoration services",
                IconUrl = "painting-icon.svg",
                DisplayOrder = 6,
                IsActive = true
            }
        };

        await context.ServiceCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        return categories;
    }

    private static async Task<List<ServiceProvider>> SeedServiceProvidersAsync(ApplicationDbContext context, List<User> users)
    {
        var providerUsers = users.Where(u => u.Role == UserRole.ServiceProvider).ToList();

        var providers = new List<ServiceProvider>
        {
            new ServiceProvider
            {
                Id = Guid.NewGuid(),
                UserId = providerUsers[0].Id,
                BusinessName = "Mohammed Plumbing Services",
                Description = "Professional plumbing services with 10 years of experience",
                LicenseNumber = "LIC-PLU-001",
                IsVerified = true,
                IsActive = true,
                IsAvailable = true,
                AverageRating = 4.8m,
                TotalReviews = 0,
                CompletedBookings = 0
            },
            new ServiceProvider
            {
                Id = Guid.NewGuid(),
                UserId = providerUsers[1].Id,
                BusinessName = "Khalid Electrical Works",
                Description = "Expert electrical services with 8 years of experience",
                LicenseNumber = "LIC-ELEC-001",
                IsVerified = true,
                IsActive = true,
                IsAvailable = true,
                AverageRating = 4.9m,
                TotalReviews = 0,
                CompletedBookings = 0
            }
        };

        await context.ServiceProviders.AddRangeAsync(providers);
        await context.SaveChangesAsync();
        return providers;
    }

    private static async Task<List<Service>> SeedServicesAsync(ApplicationDbContext context, List<ServiceCategory> categories, List<ServiceProvider> providers)
    {
        var plumbingCategory = categories.First(c => c.NameEn == "Plumbing");
        var electricalCategory = categories.First(c => c.NameEn == "Electrical");

        var services = new List<Service>
        {
            new Service
            {
                Id = Guid.NewGuid(),
                CategoryId = plumbingCategory.Id,
                ProviderId = providers[0].Id,
                NameAr = "إصلاح تسرب المياه",
                NameEn = "Water Leak Repair",
                DescriptionAr = "خدمة إصلاح تسربات المياه في الحمامات والمطابخ",
                DescriptionEn = "Water leak repair service for bathrooms and kitchens",
                BasePrice = 150.00m,
                Currency = Currency.SAR,
                EstimatedDurationMinutes = 120,
                IsActive = true,
                IsFeatured = true,
                AvailableRegions = new[] { Region.SaudiArabia },
                ImageUrls = new[] { "leak-repair-1.jpg" }
            },
            new Service
            {
                Id = Guid.NewGuid(),
                CategoryId = plumbingCategory.Id,
                ProviderId = providers[0].Id,
                NameAr = "تركيب صنبور",
                NameEn = "Faucet Installation",
                DescriptionAr = "تركيب وتبديل الصنابير",
                DescriptionEn = "Faucet installation and replacement",
                BasePrice = 100.00m,
                Currency = Currency.SAR,
                EstimatedDurationMinutes = 60,
                IsActive = true,
                IsFeatured = false,
                AvailableRegions = new[] { Region.SaudiArabia },
                ImageUrls = new[] { "faucet-install-1.jpg" }
            },
            new Service
            {
                Id = Guid.NewGuid(),
                CategoryId = electricalCategory.Id,
                ProviderId = providers[1].Id,
                NameAr = "تركيب مروحة سقف",
                NameEn = "Ceiling Fan Installation",
                DescriptionAr = "تركيب مراوح السقف وتوصيلها كهربائياً",
                DescriptionEn = "Ceiling fan installation and electrical connection",
                BasePrice = 200.00m,
                Currency = Currency.SAR,
                EstimatedDurationMinutes = 90,
                IsActive = true,
                IsFeatured = true,
                AvailableRegions = new[] { Region.SaudiArabia },
                ImageUrls = new[] { "ceiling-fan-1.jpg" }
            },
            new Service
            {
                Id = Guid.NewGuid(),
                CategoryId = electricalCategory.Id,
                ProviderId = providers[1].Id,
                NameAr = "إصلاح دائرة كهربائية",
                NameEn = "Circuit Repair",
                DescriptionAr = "إصلاح الدوائر الكهربائية المعطلة",
                DescriptionEn = "Broken electrical circuit repair",
                BasePrice = 180.00m,
                Currency = Currency.SAR,
                EstimatedDurationMinutes = 120,
                IsActive = true,
                IsFeatured = false,
                AvailableRegions = new[] { Region.SaudiArabia },
                ImageUrls = new[] { "circuit-repair-1.jpg" }
            }
        };

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
        return services;
    }

    private static async Task<List<Address>> SeedAddressesAsync(ApplicationDbContext context, List<User> users)
    {
        var customerUsers = users.Where(u => u.Role == UserRole.Customer).ToList();

        var addresses = new List<Address>
        {
            new Address
            {
                Id = Guid.NewGuid(),
                UserId = customerUsers[0].Id,
                Label = "Home",
                Street = "King Fahd Road",
                BuildingNumber = "1234",
                City = "Riyadh",
                Region = Region.SaudiArabia,
                PostalCode = "11564",
                Latitude = 24.7136,
                Longitude = 46.6753,
                IsDefault = true
            },
            new Address
            {
                Id = Guid.NewGuid(),
                UserId = customerUsers[1].Id,
                Label = "Home",
                Street = "Prince Mohammed Bin Abdulaziz St",
                BuildingNumber = "5678",
                City = "Jeddah",
                Region = Region.SaudiArabia,
                PostalCode = "23442",
                Latitude = 21.5433,
                Longitude = 39.1728,
                IsDefault = true
            }
        };

        await context.Addresses.AddRangeAsync(addresses);
        await context.SaveChangesAsync();
        return addresses;
    }

    private static async Task<List<Booking>> SeedBookingsAsync(ApplicationDbContext context, List<User> users, List<Service> services, List<Address> addresses)
    {
        var customerUsers = users.Where(u => u.Role == UserRole.Customer).ToList();

        var bookings = new List<Booking>();

        // Create 2 bookings for first customer
        for (int i = 0; i < 2; i++)
        {
            var service = services[i % services.Count];
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerUsers[0].Id,
                ServiceId = service.Id,
                ProviderId = service.ProviderId!.Value,
                AddressId = addresses[0].Id,
                Status = i == 0 ? BookingStatus.Completed : BookingStatus.Confirmed,
                TotalAmount = service.BasePrice,
                Currency = service.Currency,
                ScheduledAt = DateTime.UtcNow.AddDays(i + 1),
                Region = Region.SaudiArabia
            };
            bookings.Add(booking);
        }

        // Create 2 bookings for second customer
        for (int i = 0; i < 2; i++)
        {
            var service = services[(i + 2) % services.Count];
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerUsers[1].Id,
                ServiceId = service.Id,
                ProviderId = service.ProviderId!.Value,
                AddressId = addresses[1].Id,
                Status = i == 0 ? BookingStatus.Completed : BookingStatus.Pending,
                TotalAmount = service.BasePrice,
                Currency = service.Currency,
                ScheduledAt = DateTime.UtcNow.AddDays(i + 3),
                Region = Region.SaudiArabia
            };
            bookings.Add(booking);
        }

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();
        return bookings;
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context, List<Booking> bookings)
    {
        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();

        var reviews = new List<Review>();
        foreach (var booking in completedBookings)
        {
            reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                ProviderId = booking.ProviderId!.Value,
                Rating = 5,
                Comment = "Excellent service! Very professional and quick.",
                IsVerified = true,
                IsVisible = true
            });
        }

        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSystemConfigurationAsync(ApplicationDbContext context)
    {
        var configs = new List<SystemConfiguration>
        {
            new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Key = "PlatformCommissionRate",
                Value = "0.15",
                Description = "Platform commission rate (15%)",
                Category = "Payments"
            },
            new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Key = "MinBookingAmount",
                Value = "50",
                Description = "Minimum booking amount in SAR",
                Category = "Bookings"
            },
            new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Key = "CancellationWindowHours",
                Value = "24",
                Description = "Hours before booking when cancellation is allowed",
                Category = "Bookings"
            }
        };

        await context.SystemConfigurations.AddRangeAsync(configs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPromoCodesAsync(ApplicationDbContext context, List<Service> services, List<ServiceCategory> categories)
    {
        var promoCodes = new List<PromoCode>
        {
            new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = "WELCOME10",
                DescriptionEn = "10% off for new customers",
                DescriptionAr = "خصم 10% للعملاء الجدد",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10.0m,
                MaxDiscountAmount = 50.0m,
                MinOrderAmount = 100.0m,
                ValidFrom = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddMonths(3),
                MaxUsageCount = 1000,
                MaxUsagePerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true
            },
            new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = "FIRSTBOOKING",
                DescriptionEn = "SAR 25 off on first booking",
                DescriptionAr = "خصم 25 ريال على أول حجز",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 25.0m,
                MinOrderAmount = 150.0m,
                ValidFrom = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddMonths(6),
                MaxUsageCount = 500,
                MaxUsagePerUser = 1,
                CurrentUsageCount = 0,
                IsActive = true
            }
        };

        await context.AddRangeAsync(promoCodes);
        await context.SaveChangesAsync();
    }
}
