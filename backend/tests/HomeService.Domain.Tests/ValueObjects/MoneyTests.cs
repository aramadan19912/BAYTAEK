using FluentAssertions;
using HomeService.Domain.Enums;
using HomeService.Domain.ValueObjects;
using Xunit;

namespace HomeService.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange
        decimal amount = 100m;
        var currency = Currency.SAR;

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        decimal amount = -50m;
        var currency = Currency.SAR;

        // Act
        Action act = () => new Money(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldCreateMoney()
    {
        // Arrange
        decimal amount = 0m;
        var currency = Currency.USD;

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(100, 50, 150)]
    [InlineData(1000.50, 999.50, 2000)]
    [InlineData(0, 100, 100)]
    public void AddOperator_WithSameCurrency_ShouldReturnSum(decimal amount1, decimal amount2, decimal expected)
    {
        // Arrange
        var money1 = new Money(amount1, Currency.SAR);
        var money2 = new Money(amount2, Currency.SAR);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(expected);
        result.Currency.Should().Be(Currency.SAR);
    }

    [Fact]
    public void AddOperator_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, Currency.SAR);
        var money2 = new Money(50m, Currency.EGP);

        // Act
        Action act = () => { var result = money1 + money2; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add money with different currencies*");
    }

    [Theory]
    [InlineData(150, 50, 100)]
    [InlineData(2000, 999.50, 1000.50)]
    [InlineData(100, 100, 0)]
    public void SubtractOperator_WithSameCurrency_ShouldReturnDifference(decimal amount1, decimal amount2, decimal expected)
    {
        // Arrange
        var money1 = new Money(amount1, Currency.SAR);
        var money2 = new Money(amount2, Currency.SAR);

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(expected);
        result.Currency.Should().Be(Currency.SAR);
    }

    [Fact]
    public void SubtractOperator_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, Currency.SAR);
        var money2 = new Money(50m, Currency.EGP);

        // Act
        Action act = () => { var result = money1 - money2; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot subtract money with different currencies*");
    }

    [Theory]
    [InlineData(100, 2, 200)]
    [InlineData(50, 0.5, 25)]
    [InlineData(100, 0, 0)]
    [InlineData(75.50, 3, 226.50)]
    public void MultiplyOperator_ShouldReturnMultipliedAmount(decimal amount, decimal multiplier, decimal expected)
    {
        // Arrange
        var money = new Money(amount, Currency.SAR);

        // Act
        var result = money * multiplier;

        // Assert
        result.Amount.Should().Be(expected);
        result.Currency.Should().Be(Currency.SAR);
    }

    [Theory]
    [InlineData(100, 15, 115)]  // Saudi Arabia VAT
    [InlineData(100, 14, 114)]  // Egypt VAT
    [InlineData(200, 10, 220)]
    [InlineData(50, 5, 52.50)]
    public void AddVat_ShouldReturnAmountWithVat(decimal amount, decimal vatPercentage, decimal expected)
    {
        // Arrange
        var money = new Money(amount, Currency.SAR);

        // Act
        var result = money.AddVat(vatPercentage);

        // Assert
        result.Amount.Should().Be(expected);
        result.Currency.Should().Be(Currency.SAR);
    }

    [Fact]
    public void AddVat_WithZeroPercentage_ShouldReturnSameAmount()
    {
        // Arrange
        var money = new Money(100m, Currency.SAR);

        // Act
        var result = money.AddVat(0);

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, Currency.SAR);
        var money2 = new Money(100m, Currency.SAR);

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, Currency.SAR);
        var money2 = new Money(150m, Currency.SAR);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 == money2).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, Currency.SAR);
        var money2 = new Money(100m, Currency.EGP);

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 == money2).Should().BeFalse();
    }
}
