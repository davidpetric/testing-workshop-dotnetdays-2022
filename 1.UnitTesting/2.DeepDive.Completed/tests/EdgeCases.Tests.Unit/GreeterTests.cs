using EdgeCases.Time;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EdgeCases.Tests.Unit;

public class GreeterTests
{
    private readonly Greeter _greeter;
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    public GreeterTests()
    {
        _greeter = new Greeter(_dateTimeProvider);
    }

    [Fact]
    public void GenerateGreetText_ShouldReturnGoodMorning_WhenItsMorning()
    {
        // Arrange
        _dateTimeProvider.Now.Returns(new DateTime(2022, 1, 1, 9, 0, 0));
        
        // Act
        var message = _greeter.GenerateGreetText();

        // Assert
        message.Should().Be("Good morning");
    }
    
    [Fact]
    public void GenerateGreetText_ShouldReturnGoodAfternoon_WhenItsAfternoon()
    {
        // Arrange
        _dateTimeProvider.Now.Returns(new DateTime(2022, 1, 1, 15, 0, 0));
        
        // Act
        var message = _greeter.GenerateGreetText();

        // Assert
        message.Should().Be("Good afternoon");
    }
    
    [Fact]
    public void GenerateGreetText_ShouldReturnGoodEvening_WhenItsEvening()
    {
        // Arrange
        _dateTimeProvider.Now.Returns(new DateTime(2022, 1, 1, 20, 0, 0));
        
        // Act
        var message = _greeter.GenerateGreetText();

        // Assert
        message.Should().Be("Good evening");
    }
}
