namespace SystemProcessCotation.Tests;

public class PriceParserTests
{
    [Theory]
    [InlineData("BRL 1.234,56", 1234.56)]
    [InlineData("1.234,56 BRL", 1234.56)]
    [InlineData("brl35,50", 35.50)]
    public void TryParse_AcceptsIsoCurrencyMarkers(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }
}
