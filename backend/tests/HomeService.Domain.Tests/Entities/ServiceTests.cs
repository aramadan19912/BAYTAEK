using FluentAssertions;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using Xunit;

namespace HomeService.Domain.Tests.Entities;

public class ServiceTests
{
    [Fact]
    public void Service_ShouldInitializeWithDefaultValues()
    {
        // Act
        var service = new Service();

        // Assert
        service.NameAr.Should().BeEmpty();
        service.NameEn.Should().BeEmpty();
        service.DescriptionAr.Should().BeEmpty();
        service.DescriptionEn.Should().BeEmpty();
        service.IsActive.Should().BeFalse();
        service.IsFeatured.Should().BeFalse();
        service.AvailableRegions.Should().BeEmpty();
        service.ImageUrls.Should().BeEmpty();
        service.VideoUrl.Should().BeNull();
        service.RequiredMaterials.Should().BeNull();
        service.WarrantyInfo.Should().BeNull();
        service.Bookings.Should().BeEmpty();
    }

    [Fact]
    public void Service_ShouldSetAllProperties()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        // Act
        var service = new Service
        {
            CategoryId = categoryId,
            ProviderId = providerId,
            NameAr = "تنظيف المنزل",
            NameEn = "House Cleaning",
            DescriptionAr = "خدمة تنظيف شاملة للمنزل",
            DescriptionEn = "Comprehensive house cleaning service",
            BasePrice = 200m,
            Currency = Currency.SAR,
            EstimatedDurationMinutes = 120,
            IsActive = true,
            IsFeatured = true,
            AvailableRegions = new[] { Region.SaudiArabia, Region.SaudiArabia },
            ImageUrls = new[] { "https://example.com/image1.jpg", "https://example.com/image2.jpg" },
            VideoUrl = "https://example.com/video.mp4",
            RequiredMaterials = "Cleaning supplies provided",
            WarrantyInfo = "30-day satisfaction guarantee"
        };

        // Assert
        service.CategoryId.Should().Be(categoryId);
        service.ProviderId.Should().Be(providerId);
        service.NameAr.Should().Be("تنظيف المنزل");
        service.NameEn.Should().Be("House Cleaning");
        service.DescriptionAr.Should().Be("خدمة تنظيف شاملة للمنزل");
        service.DescriptionEn.Should().Be("Comprehensive house cleaning service");
        service.BasePrice.Should().Be(200m);
        service.Currency.Should().Be(Currency.SAR);
        service.EstimatedDurationMinutes.Should().Be(120);
        service.IsActive.Should().BeTrue();
        service.IsFeatured.Should().BeTrue();
        service.AvailableRegions.Should().HaveCount(2);
        service.ImageUrls.Should().HaveCount(2);
        service.VideoUrl.Should().Be("https://example.com/video.mp4");
        service.RequiredMaterials.Should().Be("Cleaning supplies provided");
        service.WarrantyInfo.Should().Be("30-day satisfaction guarantee");
    }

    [Fact]
    public void Service_ShouldSupportArabicAndEnglishNames()
    {
        // Arrange & Act
        var service = new Service
        {
            NameAr = "إصلاح السباكة",
            NameEn = "Plumbing Repair"
        };

        // Assert
        service.NameAr.Should().Be("إصلاح السباكة");
        service.NameEn.Should().Be("Plumbing Repair");
    }

    [Fact]
    public void Service_ShouldSupportArabicAndEnglishDescriptions()
    {
        // Arrange & Act
        var service = new Service
        {
            DescriptionAr = "خدمة إصلاح السباكة المتخصصة",
            DescriptionEn = "Specialized plumbing repair service"
        };

        // Assert
        service.DescriptionAr.Should().Be("خدمة إصلاح السباكة المتخصصة");
        service.DescriptionEn.Should().Be("Specialized plumbing repair service");
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void Service_ShouldSupportVariousPricePoints(decimal price)
    {
        // Arrange
        var service = new Service();

        // Act
        service.BasePrice = price;

        // Assert
        service.BasePrice.Should().Be(price);
    }

    [Theory]
    [InlineData(Currency.SAR)]
    [InlineData(Currency.EGP)]
    [InlineData(Currency.USD)]
    public void Service_ShouldSupportMultipleCurrencies(Currency currency)
    {
        // Arrange
        var service = new Service();

        // Act
        service.Currency = currency;

        // Assert
        service.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(30)]   // 30 minutes
    [InlineData(60)]   // 1 hour
    [InlineData(120)]  // 2 hours
    [InlineData(240)]  // 4 hours
    [InlineData(480)]  // 8 hours
    public void Service_ShouldSupportVariousDurations(int durationMinutes)
    {
        // Arrange
        var service = new Service();

        // Act
        service.EstimatedDurationMinutes = durationMinutes;

        // Assert
        service.EstimatedDurationMinutes.Should().Be(durationMinutes);
    }

    [Fact]
    public void Service_ActiveService_ShouldBeBookable()
    {
        // Arrange & Act
        var service = new Service
        {
            IsActive = true,
            NameEn = "Active Service",
            BasePrice = 100m
        };

        // Assert
        service.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Service_InactiveService_ShouldNotBeBookable()
    {
        // Arrange & Act
        var service = new Service
        {
            IsActive = false,
            NameEn = "Inactive Service"
        };

        // Assert
        service.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Service_FeaturedService_ShouldBeHighlighted()
    {
        // Arrange & Act
        var service = new Service
        {
            IsFeatured = true,
            NameEn = "Featured Service"
        };

        // Assert
        service.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public void Service_ShouldSupportMultipleRegions()
    {
        // Arrange & Act
        var service = new Service
        {
            AvailableRegions = new[]
            {
                Region.SaudiArabia,
                Region.SaudiArabia,
                Region.SaudiArabia,
                Region.Egypt,
                Region.Egypt
            }
        };

        // Assert
        service.AvailableRegions.Should().HaveCount(5);
        service.AvailableRegions.Should().Contain(Region.SaudiArabia);
        service.AvailableRegions.Should().Contain(Region.Egypt);
    }

    [Fact]
    public void Service_ShouldSupportMultipleImages()
    {
        // Arrange & Act
        var service = new Service
        {
            ImageUrls = new[]
            {
                "https://example.com/image1.jpg",
                "https://example.com/image2.jpg",
                "https://example.com/image3.jpg"
            }
        };

        // Assert
        service.ImageUrls.Should().HaveCount(3);
        service.ImageUrls[0].Should().Be("https://example.com/image1.jpg");
    }

    [Fact]
    public void Service_VideoUrl_ShouldBeOptional()
    {
        // Arrange & Act
        var serviceWithVideo = new Service
        {
            VideoUrl = "https://example.com/demo.mp4"
        };
        var serviceWithoutVideo = new Service
        {
            VideoUrl = null
        };

        // Assert
        serviceWithVideo.VideoUrl.Should().Be("https://example.com/demo.mp4");
        serviceWithoutVideo.VideoUrl.Should().BeNull();
    }

    [Fact]
    public void Service_RequiredMaterials_ShouldBeOptional()
    {
        // Arrange & Act
        var serviceWithMaterials = new Service
        {
            RequiredMaterials = "Cleaning supplies, mop, vacuum"
        };
        var serviceWithoutMaterials = new Service
        {
            RequiredMaterials = null
        };

        // Assert
        serviceWithMaterials.RequiredMaterials.Should().Be("Cleaning supplies, mop, vacuum");
        serviceWithoutMaterials.RequiredMaterials.Should().BeNull();
    }

    [Fact]
    public void Service_WarrantyInfo_ShouldBeOptional()
    {
        // Arrange & Act
        var serviceWithWarranty = new Service
        {
            WarrantyInfo = "90-day workmanship warranty"
        };
        var serviceWithoutWarranty = new Service
        {
            WarrantyInfo = null
        };

        // Assert
        serviceWithWarranty.WarrantyInfo.Should().Be("90-day workmanship warranty");
        serviceWithoutWarranty.WarrantyInfo.Should().BeNull();
    }

    [Fact]
    public void Service_ShouldSupportMultipleBookings()
    {
        // Arrange
        var service = new Service();
        var booking1 = new Booking
        {
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status = BookingStatus.Pending
        };
        var booking2 = new Booking
        {
            ScheduledAt = DateTime.UtcNow.AddDays(2),
            Status = BookingStatus.Confirmed
        };

        // Act
        service.Bookings.Add(booking1);
        service.Bookings.Add(booking2);

        // Assert
        service.Bookings.Should().HaveCount(2);
        service.Bookings.Should().Contain(booking1);
        service.Bookings.Should().Contain(booking2);
    }

    [Fact]
    public void Service_ProviderId_ShouldBeOptional()
    {
        // Arrange & Act
        var serviceWithProvider = new Service
        {
            ProviderId = Guid.NewGuid()
        };
        var serviceWithoutProvider = new Service
        {
            ProviderId = null
        };

        // Assert
        serviceWithProvider.ProviderId.Should().NotBeNull();
        serviceWithoutProvider.ProviderId.Should().BeNull();
    }

    [Fact]
    public void Service_ShouldBelongToCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var service = new Service
        {
            CategoryId = categoryId
        };

        // Assert
        service.CategoryId.Should().Be(categoryId);
    }
}
