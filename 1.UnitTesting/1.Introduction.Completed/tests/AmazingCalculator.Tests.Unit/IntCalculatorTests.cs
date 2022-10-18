using System.Collections;
using FluentAssertions;
using Xunit;

namespace AmazingCalculator.Tests.Unit;

public class IntCalculatorTests
{
    [Fact]
    public void Add_ShouldAddTwoNumbers_WhenTheNumbersAreIntegers()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Add(1, 5);
        
        // Assert
        result.Should().Be(6);
    }
    
    [Fact]
    public void Add_ShouldReturnZero_WhenAnOppositePositiveAndNegativeNumberAreAdded()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Add(5, -5);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Theory]
    // [InlineData(5, 3, 2)]
    // [InlineData(5, 5, 0)]
    // [InlineData(3, 5, -2)]
    //[MemberData(nameof(SubtractData))]
    [ClassData(typeof(SubtractData))]
    public void Subtract_ShouldSubtractTwoNumbers_WhenTheNumbersAreIntegers(
        int a, int b, int final)
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Subtract(a, b);

        // Assert
        result.Should().Be(final);
    }
    
    [Fact]
    public void Multiply_ShouldMultiplyTwoNumbers_WhenTheNumbersArePositiveIntegers()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Multiply(6, 9);
        
        // Assert
        result.Should().Be(54);
    }
    
    [Fact]
    public void Multiply_ShouldReturnZero_WhenOneOfTheNumbersIsZero()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Multiply(7, 0);
        
        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
    public void Divide_ShouldDivideTwoNumbers_WhenNumbersAreDivisible()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Divide(10, 2);
        
        // Assert
        result.Should().Be(5);
    }
    
    [Fact]
    public void Divide_ShouldReturnTheFirstNumber_WhenNumberIsDividedByOne()
    {
        // Arrange
        var calculator = new IntCalculator();

        // Act
        var result = calculator.Divide(7, 1);
        
        // Assert
        result.Should().Be(7);
    }

    public static IEnumerable<object?[]> SubtractData => new List<object?[]>
    {
        new object[] { 5, 3, 2 },
        new object[] { 5, 5, 0 },
        new object[] { 3, 5, -2 }
    };
}

public class SubtractData : IEnumerable<object?[]>
{
    public IEnumerator<object?[]> GetEnumerator()
    {
        yield return new object[] { 5, 3, 2 };
        yield return new object[] { 5, 5, 0 };
        yield return new object[] { 3, 5, -2 };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
