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

    [Fact]
    public void IsValid_ReturnsTrueWhenSymbolAndPriceArePresent()
    {
        var cotation = new global::CotationResult
        {
            Symbol = "PETR4",
            Price = 10.00
        };

        Assert.True(cotation.IsValid);
    }
}
