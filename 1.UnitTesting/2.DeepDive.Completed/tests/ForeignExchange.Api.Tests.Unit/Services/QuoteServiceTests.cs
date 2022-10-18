using FluentAssertions;
using ForeignExchange.Api.Logging;
using ForeignExchange.Api.Models;
using ForeignExchange.Api.Repositories;
using ForeignExchange.Api.Services;
using ForeignExchange.Api.Validation;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ForeignExchange.Api.Tests.Unit.Services;

public class QuoteServiceTests
{
    private readonly QuoteService _sut;
    private readonly IRatesRepository _ratesRepository = Substitute.For<IRatesRepository>();
    private readonly ILoggerAdapter<QuoteService> _logger = Substitute.For<ILoggerAdapter<QuoteService>>();

    public QuoteServiceTests()
    {
        _sut = new(_ratesRepository, _logger);
    }

    [Fact]
    public async Task GetQuoteAsync_ReturnsQuote_WhenCurrenciesAreValid()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "USD";
        var amount = 100;
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = 1.6m
        };

        var expectedQuote = new ConversionQuote
        {
            BaseCurrency = fromCurrency,
            QuoteCurrency = toCurrency,
            BaseAmount = amount,
            QuoteAmount = 160
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);
        
        // Act
        var quote = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        quote!.Should().BeEquivalentTo(expectedQuote);
    }
    
    [Theory]
    [InlineData("GBP", "EUR", 100, 1.6, 160)]
    public async Task GetQuoteAsync_ReturnsQuote_WhenCurrenciesAreValid2(
        string fromCurrency, string toCurrency, decimal amount, decimal rate, decimal final)
    {
        // Arrange
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = rate
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);
        
        // Act
        var quote = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        quote!.QuoteAmount.Should().Be(final);
    }
    
    [Fact]
    public async Task GetQuoteAsync_ThrowsException_WhenSameCurrencyIsUsed()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "GBP";
        var amount = 100;
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = 1.6m
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);
        
        // Act
        var quoteAction = () => _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        await quoteAction.Should().ThrowAsync<SameCurrencyException>()
            .WithMessage($"You cannot convert currency {fromCurrency} to itself");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetQuoteAsync_ThrowsException_WhenAmountIsZeroOrNegative(decimal amount)
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "USD";
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = 1.6m
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);
        
        // Act
        var quoteAction = () => _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        await quoteAction.Should().ThrowAsync<NegativeAmountException>()
            .WithMessage("You can only convert a positive amount of money");
    }
    
    [Fact]
    public async Task GetQuoteAsync_LogsAppropriateMessage_WhenInvoked()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "USD";
        var amount = 100;
        var expectedRate = new FxRate
        {
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            TimestampUtc = DateTime.UtcNow,
            Rate = 1.6m
        };
        
        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .Returns(expectedRate);
        
        // Act
        await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        _logger.Received(1)
            .LogInformation("Retrieved quote for currencies {FromCurrency}->{ToCurrency} in {ElapsedMilliseconds}ms",
                Arg.Is<object[]>(x => 
                    x[0].ToString() == fromCurrency && 
                    x[1].ToString() == toCurrency));
    }
    
    [Fact]
    public async Task GetQuoteAsync_ReturnsNull_WhenNoRateExists()
    {
        // Arrange
        var fromCurrency = "GBP";
        var toCurrency = "USD";
        var amount = 100;

        _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
            .ReturnsNull();
        
        // Act
        var quote = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

        // Assert
        quote.Should().BeNull();
    }
}
