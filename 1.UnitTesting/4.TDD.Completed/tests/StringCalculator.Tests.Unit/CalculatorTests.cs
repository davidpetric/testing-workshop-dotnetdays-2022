using FluentAssertions;
using Xunit;

namespace StringCalculator.Tests.Unit;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldAddTwoNumbers_WhenTheyAreSplitByComma()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("1,2");

        // Assert
        result.Should().Be(3);
    }
    
    [Fact]
    public void Add_ShouldReturnZero_WhenTheStringIsEmpty()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(string.Empty);

        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public void Add_ShouldReturnTheNumber_WhenTheStringIsASingleNumber()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("1");

        // Assert
        result.Should().Be(1);
    }
    
    [Fact]
    public void Add_ShouldAddMultipleNumbers_WhenTheyAreSplitByComma()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("1,2,3");

        // Assert
        result.Should().Be(6);
    }
    
    [Fact]
    public void Add_ShouldAddMultipleNumbers_WhenTheyAreSplitByCommaAndNewLine()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("1\n2,3");

        // Assert
        result.Should().Be(6);
    }
    
    [Fact]
    public void Add_ShouldAddMultipleNumbers_WhenACustomDelimiterIsUsed()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("//;\n1;2");

        // Assert
        result.Should().Be(3);
    }
    
    [Fact]
    public void Add_ShouldThrowNegativesNotAllowedException_WhenNegativesAreUsed()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = () => calculator.Add("//;\n-1;2;-5");

        // Assert
        result.Should().Throw<NegativesNotAllowedException>()
            .WithMessage("Negative numbers such as -1, -5 are not allowed.");
    }
    
    [Fact]
    public void Add_ShouldIgnoreNumbers_WhenTheNumbersAreOver1000()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add("//;\n2;1001;1");

        // Assert
        result.Should().Be(3);
    }
}
