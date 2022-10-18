using FluentAssertions;
using Xunit;

namespace AwesomeCalculator.Tests.Unit;

public class CalculatorTests
{
    [Theory]
    [InlineData(5, 5, 10)]
    public void Test_Add(int first, int second, int expected)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = sut.Add(first, second);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 5, 0)]
    [InlineData(10, 9, 1)]
    public void Test_Subtract(int first, int second, int expected)
    {
        // Arrange
        var sut = new Calculator();
        
        // Act
        var result = sut.Subtract(first, second);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(2, 3, 6)]
    public void Test_Multiply(int first, int second, int expected)
    {
        // Arrange
        var sut = new Calculator();
        
        // Act
        var result = sut.Multiply(first, second);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1, 1, 0)]
    [InlineData(4, 2, 2, 0)]
    [InlineData(5, 2, 2, 1)]
    public void Test_Divide(int first, int second, int expected, int remainder)
    {
        // Arrange
        var sut = new Calculator();
        
        // Act
        var result = sut.Divide(first, second);

        // Assert
        result.Result.Should().Be(expected);
        result.Remainder.Should().Be(remainder);
    }

    [Fact]
    public void Test_Divide_ByZero()
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = () => sut.Divide(1, 0);
        
        // Assert
        result.Should().Throw<DivideByZeroException>();
    }
}
