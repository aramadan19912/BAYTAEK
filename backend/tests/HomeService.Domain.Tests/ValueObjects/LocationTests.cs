using FluentAssertions;
using HomeService.Domain.ValueObjects;
using Xunit;

namespace HomeService.Domain.Tests.ValueObjects;

public class LocationTests
{
    [Theory]
    [InlineData(24.7136, 46.6753)] // Riyadh
    [InlineData(30.0444, 31.2357)] // Cairo
    [InlineData(-33.8688, 151.2093)] // Sydney
    [InlineData(0, 0)] // Equator/Prime Meridian
    [InlineData(-90, -180)] // Min values
    [InlineData(90, 180)] // Max values
    public void Constructor_WithValidCoordinates_ShouldCreateLocation(double latitude, double longitude)
    {
        // Act
        var location = new Location(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(-100, 0)]
    [InlineData(100, 0)]
    public void Constructor_WithInvalidLatitude_ShouldThrowArgumentException(double latitude, double longitude)
    {
        // Act
        Action act = () => new Location(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Latitude must be between -90 and 90*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [InlineData(0, -200)]
    [InlineData(0, 200)]
    public void Constructor_WithInvalidLongitude_ShouldThrowArgumentException(double latitude, double longitude)
    {
        // Act
        Action act = () => new Location(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Longitude must be between -180 and 180*");
    }

    [Fact]
    public void DistanceTo_SameLocation_ShouldReturnZero()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753); // Riyadh
        var location2 = new Location(24.7136, 46.6753); // Same Riyadh coordinates

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceTo_RiyadhToJeddah_ShouldReturnApproximateDistance()
    {
        // Arrange
        var riyadh = new Location(24.7136, 46.6753);
        var jeddah = new Location(21.4858, 39.1925);

        // Act
        var distance = riyadh.DistanceTo(jeddah);

        // Assert
        // Approximate distance between Riyadh and Jeddah is ~870 km
        distance.Should().BeInRange(850, 900);
    }

    [Fact]
    public void DistanceTo_CairoToAlexandria_ShouldReturnApproximateDistance()
    {
        // Arrange
        var cairo = new Location(30.0444, 31.2357);
        var alexandria = new Location(31.2001, 29.9187);

        // Act
        var distance = cairo.DistanceTo(alexandria);

        // Assert
        // Approximate distance between Cairo and Alexandria is ~180 km
        distance.Should().BeInRange(170, 200);
    }

    [Fact]
    public void DistanceTo_RiyadhToCairo_ShouldReturnApproximateDistance()
    {
        // Arrange
        var riyadh = new Location(24.7136, 46.6753);
        var cairo = new Location(30.0444, 31.2357);

        // Act
        var distance = riyadh.DistanceTo(cairo);

        // Assert
        // Approximate distance between Riyadh and Cairo is ~1400 km
        distance.Should().BeInRange(1350, 1450);
    }

    [Fact]
    public void DistanceTo_AcrossEquator_ShouldCalculateCorrectly()
    {
        // Arrange
        var northernLocation = new Location(10, 0);
        var southernLocation = new Location(-10, 0);

        // Act
        var distance = northernLocation.DistanceTo(southernLocation);

        // Assert
        // Distance should be approximately 2223 km (20 degrees * ~111 km per degree)
        distance.Should().BeInRange(2200, 2250);
    }

    [Fact]
    public void DistanceTo_IsSymmetric_ShouldReturnSameDistance()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753);
        var location2 = new Location(21.4858, 39.1925);

        // Act
        var distance1to2 = location1.DistanceTo(location2);
        var distance2to1 = location2.DistanceTo(location1);

        // Assert
        distance1to2.Should().BeApproximately(distance2to1, 0.01);
    }

    [Fact]
    public void Equality_WithSameCoordinates_ShouldBeEqual()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753);
        var location2 = new Location(24.7136, 46.6753);

        // Act & Assert
        location1.Should().Be(location2);
        (location1 == location2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentLatitude_ShouldNotBeEqual()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753);
        var location2 = new Location(25.0000, 46.6753);

        // Act & Assert
        location1.Should().NotBe(location2);
        (location1 == location2).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentLongitude_ShouldNotBeEqual()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753);
        var location2 = new Location(24.7136, 47.0000);

        // Act & Assert
        location1.Should().NotBe(location2);
        (location1 == location2).Should().BeFalse();
    }

    [Fact]
    public void DistanceTo_VeryCloseLocations_ShouldReturnSmallDistance()
    {
        // Arrange
        var location1 = new Location(24.7136, 46.6753);
        var location2 = new Location(24.7137, 46.6754); // Very close (about 100 meters)

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        distance.Should().BeLessThan(1); // Less than 1 km
        distance.Should().BeGreaterThan(0);
    }
}
