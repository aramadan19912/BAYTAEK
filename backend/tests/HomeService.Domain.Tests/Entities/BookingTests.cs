using FluentAssertions;
using HomeService.Domain.Entities;
using HomeService.Domain.Enums;
using Xunit;

namespace HomeService.Domain.Tests.Entities;

public class BookingTests
{
    [Fact]
    public void Booking_ShouldInitializeWithDefaultValues()
    {
        // Act
        var booking = new Booking();

        // Assert
        booking.Status.Should().Be(default(BookingStatus));
        booking.StartedAt.Should().BeNull();
        booking.CompletedAt.Should().BeNull();
        booking.CancelledAt.Should().BeNull();
        booking.CancellationReason.Should().BeNull();
        booking.SpecialInstructions.Should().BeNull();
        booking.IsRecurring.Should().BeFalse();
        booking.RecurrencePattern.Should().BeNull();
        booking.Payment.Should().BeNull();
        booking.Review.Should().BeNull();
    }

    [Fact]
    public void Booking_ShouldSetAllProperties()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        // Act
        var booking = new Booking
        {
            CustomerId = customerId,
            ServiceId = serviceId,
            ProviderId = providerId,
            AddressId = addressId,
            Status = BookingStatus.Pending,
            ScheduledAt = scheduledAt,
            TotalAmount = 500m,
            VatAmount = 75m,
            VatPercentage = 15m,
            Currency = Currency.SAR,
            SpecialInstructions = "Please call before arrival",
            IsRecurring = false
        };

        // Assert
        booking.CustomerId.Should().Be(customerId);
        booking.ServiceId.Should().Be(serviceId);
        booking.ProviderId.Should().Be(providerId);
        booking.AddressId.Should().Be(addressId);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.ScheduledAt.Should().Be(scheduledAt);
        booking.TotalAmount.Should().Be(500m);
        booking.VatAmount.Should().Be(75m);
        booking.VatPercentage.Should().Be(15m);
        booking.Currency.Should().Be(Currency.SAR);
        booking.SpecialInstructions.Should().Be("Please call before arrival");
        booking.IsRecurring.Should().BeFalse();
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.InProgress)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Rejected)]
    public void Booking_ShouldSupportAllStatuses(BookingStatus status)
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.Status = status;

        // Assert
        booking.Status.Should().Be(status);
    }

    [Fact]
    public void Booking_WhenConfirmed_ShouldHaveProvider()
    {
        // Arrange & Act
        var booking = new Booking
        {
            Status = BookingStatus.Confirmed,
            ProviderId = Guid.NewGuid()
        };

        // Assert
        booking.ProviderId.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Booking_WhenInProgress_ShouldHaveStartedAt()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var booking = new Booking
        {
            Status = BookingStatus.InProgress,
            StartedAt = startTime
        };

        // Assert
        booking.Status.Should().Be(BookingStatus.InProgress);
        booking.StartedAt.Should().Be(startTime);
        booking.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Booking_WhenCompleted_ShouldHaveCompletedAt()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-2);
        var completedTime = DateTime.UtcNow;

        // Act
        var booking = new Booking
        {
            Status = BookingStatus.Completed,
            StartedAt = startTime,
            CompletedAt = completedTime
        };

        // Assert
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.StartedAt.Should().Be(startTime);
        booking.CompletedAt.Should().Be(completedTime);
        booking.CompletedAt.Should().BeAfter(booking.StartedAt.Value);
    }

    [Fact]
    public void Booking_WhenCancelled_ShouldHaveCancellationDetails()
    {
        // Arrange
        var cancelledTime = DateTime.UtcNow;
        var reason = "Customer requested cancellation";

        // Act
        var booking = new Booking
        {
            Status = BookingStatus.Cancelled,
            CancelledAt = cancelledTime,
            CancellationReason = reason
        };

        // Assert
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancelledAt.Should().Be(cancelledTime);
        booking.CancellationReason.Should().Be(reason);
    }

    [Theory]
    [InlineData(100, 15, 15)]  // SAR with 15% VAT
    [InlineData(100, 14, 14)]  // EGP with 14% VAT
    [InlineData(500, 15, 75)]
    [InlineData(1000, 14, 140)]
    public void Booking_VatCalculation_ShouldBeCorrect(decimal totalBeforeVat, decimal vatPercentage, decimal expectedVat)
    {
        // Arrange & Act
        var booking = new Booking
        {
            TotalAmount = totalBeforeVat + expectedVat,
            VatAmount = expectedVat,
            VatPercentage = vatPercentage
        };

        // Assert
        booking.VatAmount.Should().Be(expectedVat);
        booking.VatPercentage.Should().Be(vatPercentage);
        booking.TotalAmount.Should().Be(totalBeforeVat + expectedVat);
    }

    [Theory]
    [InlineData(Currency.SAR)]
    [InlineData(Currency.EGP)]
    [InlineData(Currency.USD)]
    public void Booking_ShouldSupportMultipleCurrencies(Currency currency)
    {
        // Arrange
        var booking = new Booking();

        // Act
        booking.Currency = currency;

        // Assert
        booking.Currency.Should().Be(currency);
    }

    [Fact]
    public void Booking_RecurringBooking_ShouldHavePattern()
    {
        // Arrange & Act
        var booking = new Booking
        {
            IsRecurring = true,
            RecurrencePattern = "Weekly on Monday"
        };

        // Assert
        booking.IsRecurring.Should().BeTrue();
        booking.RecurrencePattern.Should().Be("Weekly on Monday");
    }

    [Fact]
    public void Booking_NonRecurringBooking_ShouldNotHavePattern()
    {
        // Arrange & Act
        var booking = new Booking
        {
            IsRecurring = false,
            RecurrencePattern = null
        };

        // Assert
        booking.IsRecurring.Should().BeFalse();
        booking.RecurrencePattern.Should().BeNull();
    }

    [Fact]
    public void Booking_WithPayment_ShouldLinkCorrectly()
    {
        // Arrange
        var booking = new Booking();
        var payment = new Payment
        {
            Amount = 500m,
            Status = PaymentStatus.Completed
        };

        // Act
        booking.Payment = payment;

        // Assert
        booking.Payment.Should().NotBeNull();
        booking.Payment.Amount.Should().Be(500m);
        booking.Payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Booking_WithReview_ShouldLinkCorrectly()
    {
        // Arrange
        var booking = new Booking
        {
            Status = BookingStatus.Completed
        };
        var review = new Review
        {
            Rating = 5,
            Comment = "Excellent service!"
        };

        // Act
        booking.Review = review;

        // Assert
        booking.Review.Should().NotBeNull();
        booking.Review.Rating.Should().Be(5);
        booking.Review.Comment.Should().Be("Excellent service!");
    }

    [Fact]
    public void Booking_ScheduledInFuture_ShouldBeValid()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var booking = new Booking
        {
            ScheduledAt = futureDate,
            Status = BookingStatus.Pending
        };

        // Assert
        booking.ScheduledAt.Should().BeAfter(DateTime.UtcNow);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public void Booking_SpecialInstructions_ShouldBeOptional()
    {
        // Arrange & Act
        var bookingWithInstructions = new Booking
        {
            SpecialInstructions = "Ring the doorbell twice"
        };
        var bookingWithoutInstructions = new Booking
        {
            SpecialInstructions = null
        };

        // Assert
        bookingWithInstructions.SpecialInstructions.Should().Be("Ring the doorbell twice");
        bookingWithoutInstructions.SpecialInstructions.Should().BeNull();
    }

    [Fact]
    public void Booking_ProviderId_ShouldBeOptionalForPendingStatus()
    {
        // Arrange & Act
        var booking = new Booking
        {
            Status = BookingStatus.Pending,
            ProviderId = null
        };

        // Assert
        booking.ProviderId.Should().BeNull();
        booking.Status.Should().Be(BookingStatus.Pending);
    }
}
