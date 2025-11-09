using FluentAssertions;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using Xunit;

namespace HomeService.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.PhoneNumber.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.IsEmailVerified.Should().BeFalse();
        user.IsPhoneVerified.Should().BeFalse();
        user.IsTwoFactorEnabled.Should().BeFalse();
        user.PreferredLanguage.Should().Be(Language.Arabic);
        user.ProfileImageUrl.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
        user.Addresses.Should().BeEmpty();
        user.Bookings.Should().BeEmpty();
        user.Reviews.Should().BeEmpty();
        user.Notifications.Should().BeEmpty();
        user.ServiceProvider.Should().BeNull();
    }

    [Fact]
    public void User_ShouldSetProperties()
    {
        // Arrange
        var user = new User();
        var now = DateTime.UtcNow;

        // Act
        user.FirstName = "Ahmed";
        user.LastName = "Ali";
        user.Email = "ahmed.ali@example.com";
        user.PhoneNumber = "+966501234567";
        user.PasswordHash = "hashedpassword123";
        user.Role = UserRole.Customer;
        user.IsEmailVerified = true;
        user.IsPhoneVerified = true;
        user.IsTwoFactorEnabled = true;
        user.PreferredLanguage = Language.English;
        user.Region = Region.Riyadh;
        user.ProfileImageUrl = "https://example.com/profile.jpg";
        user.LastLoginAt = now;

        // Assert
        user.FirstName.Should().Be("Ahmed");
        user.LastName.Should().Be("Ali");
        user.Email.Should().Be("ahmed.ali@example.com");
        user.PhoneNumber.Should().Be("+966501234567");
        user.PasswordHash.Should().Be("hashedpassword123");
        user.Role.Should().Be(UserRole.Customer);
        user.IsEmailVerified.Should().BeTrue();
        user.IsPhoneVerified.Should().BeTrue();
        user.IsTwoFactorEnabled.Should().BeTrue();
        user.PreferredLanguage.Should().Be(Language.English);
        user.Region.Should().Be(Region.Riyadh);
        user.ProfileImageUrl.Should().Be("https://example.com/profile.jpg");
        user.LastLoginAt.Should().Be(now);
    }

    [Theory]
    [InlineData(UserRole.Customer)]
    [InlineData(UserRole.ServiceProvider)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.SuperAdmin)]
    public void User_ShouldSupportAllRoles(UserRole role)
    {
        // Arrange
        var user = new User();

        // Act
        user.Role = role;

        // Assert
        user.Role.Should().Be(role);
    }

    [Theory]
    [InlineData(Language.Arabic)]
    [InlineData(Language.English)]
    public void User_ShouldSupportLanguagePreferences(Language language)
    {
        // Arrange
        var user = new User();

        // Act
        user.PreferredLanguage = language;

        // Assert
        user.PreferredLanguage.Should().Be(language);
    }

    [Theory]
    [InlineData(Region.Riyadh)]
    [InlineData(Region.Makkah)]
    [InlineData(Region.Madinah)]
    [InlineData(Region.Cairo)]
    [InlineData(Region.Alexandria)]
    public void User_ShouldSupportAllRegions(Region region)
    {
        // Arrange
        var user = new User();

        // Act
        user.Region = region;

        // Assert
        user.Region.Should().Be(region);
    }

    [Fact]
    public void User_ShouldSupportMultipleAddresses()
    {
        // Arrange
        var user = new User();
        var address1 = new Address
        {
            Label = "Home",
            AddressLine = "123 Main St",
            City = "Riyadh"
        };
        var address2 = new Address
        {
            Label = "Work",
            AddressLine = "456 Business Ave",
            City = "Riyadh"
        };

        // Act
        user.Addresses.Add(address1);
        user.Addresses.Add(address2);

        // Assert
        user.Addresses.Should().HaveCount(2);
        user.Addresses.Should().Contain(address1);
        user.Addresses.Should().Contain(address2);
    }

    [Fact]
    public void User_ShouldSupportMultipleBookings()
    {
        // Arrange
        var user = new User();
        var booking1 = new Booking
        {
            ScheduledDateTime = DateTime.UtcNow.AddDays(1),
            Status = BookingStatus.Pending
        };
        var booking2 = new Booking
        {
            ScheduledDateTime = DateTime.UtcNow.AddDays(2),
            Status = BookingStatus.Confirmed
        };

        // Act
        user.Bookings.Add(booking1);
        user.Bookings.Add(booking2);

        // Assert
        user.Bookings.Should().HaveCount(2);
        user.Bookings.Should().Contain(booking1);
        user.Bookings.Should().Contain(booking2);
    }

    [Fact]
    public void User_ShouldSupportMultipleReviews()
    {
        // Arrange
        var user = new User();
        var review1 = new Review
        {
            Rating = 5,
            Comment = "Excellent service!"
        };
        var review2 = new Review
        {
            Rating = 4,
            Comment = "Very good"
        };

        // Act
        user.Reviews.Add(review1);
        user.Reviews.Add(review2);

        // Assert
        user.Reviews.Should().HaveCount(2);
        user.Reviews.Should().Contain(review1);
        user.Reviews.Should().Contain(review2);
    }

    [Fact]
    public void User_WithServiceProviderRole_CanHaveServiceProviderProfile()
    {
        // Arrange
        var user = new User
        {
            Role = UserRole.ServiceProvider
        };
        var serviceProvider = new ServiceProvider
        {
            Bio = "Experienced technician",
            IsVerified = true
        };

        // Act
        user.ServiceProvider = serviceProvider;

        // Assert
        user.ServiceProvider.Should().NotBeNull();
        user.ServiceProvider.Bio.Should().Be("Experienced technician");
        user.ServiceProvider.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void User_EmailVerification_ShouldBeIndependent()
    {
        // Arrange
        var user = new User
        {
            IsEmailVerified = true,
            IsPhoneVerified = false
        };

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.IsPhoneVerified.Should().BeFalse();
    }

    [Fact]
    public void User_PhoneVerification_ShouldBeIndependent()
    {
        // Arrange
        var user = new User
        {
            IsEmailVerified = false,
            IsPhoneVerified = true
        };

        // Assert
        user.IsEmailVerified.Should().BeFalse();
        user.IsPhoneVerified.Should().BeTrue();
    }

    [Fact]
    public void User_TwoFactorAuthentication_ShouldBeOptional()
    {
        // Arrange
        var userWithout2FA = new User
        {
            IsTwoFactorEnabled = false
        };
        var userWith2FA = new User
        {
            IsTwoFactorEnabled = true
        };

        // Assert
        userWithout2FA.IsTwoFactorEnabled.Should().BeFalse();
        userWith2FA.IsTwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public void User_LastLoginAt_ShouldBeNullableAndTrackable()
    {
        // Arrange
        var user = new User();
        var loginTime = DateTime.UtcNow;

        // Act - Initial state
        user.LastLoginAt.Should().BeNull();

        // Act - After login
        user.LastLoginAt = loginTime;

        // Assert
        user.LastLoginAt.Should().Be(loginTime);
    }
}
