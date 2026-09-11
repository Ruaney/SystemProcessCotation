namespace SystemProcessCotation.Tests;

public class CotationResultTests
{
    [Fact]
    public void Constructor_DefaultsTimestampToUtc()
    {
        var cotation = new global::CotationResult();

        Assert.Equal(DateTimeKind.Utc, cotation.Timestamp.Kind);
    }

    [Fact]
    public void IsValid_ReturnsFalseWhenSymbolIsWhitespace()
    {
        var cotation = new global::CotationResult
        {
            Symbol = "   ",
            Price = 10.00
        };

        Assert.False(cotation.IsValid);
    }

    [Theory]
    [InlineData("PETR 4")]
    [InlineData("PETR-4")]
    public void IsValid_ReturnsFalseWhenSymbolHasInvalidCharacters(string symbol)
    {
        var cotation = new global::CotationResult
        {
            Symbol = symbol,
            Price = 10.00
        };

        Assert.False(cotation.IsValid);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void IsValid_ReturnsFalseWhenPriceIsNotFinite(double price)
    {
        var cotation = new global::CotationResult
        {
            Symbol = "PETR4",
            Price = price
        };

        Assert.False(cotation.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsTrueWhenSymbolAndPriceArePresent()
    {
        var cotation = new global::CotationResult
        {
            Symbol = " petr4 ",
            Price = 10.00
        };

        Assert.True(cotation.IsValid);
    }
}
