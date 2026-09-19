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

    [Theory]
    [InlineData("Atualizado em 19/09/2026: R$ 31,42", 31.42)]
    [InlineData("19/09/2026 BRL31,42", 31.42)]
    [InlineData("31,42 BRL atualizado em 19/09/2026", 31.42)]
    public void TryParse_PrefersCurrencyMarkedPriceWhenTextContainsOtherNumbers(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("1.234", 1234.00)]
    [InlineData("1,234", 1234.00)]
    [InlineData("R$ 12.345", 12345.00)]
    [InlineData("BRL 12,345", 12345.00)]
    public void TryParse_TreatsSingleThreeDigitSeparatorAsThousands(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("1.234.567,89", 1234567.89)]
    [InlineData("R$ 12.345.678", 12345678.00)]
    public void TryParse_AcceptsGroupedBrazilianThousands(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("31,42 +0,50%", 31.42)]
    [InlineData("+0,50% R$ 31,42", 31.42)]
    public void TryParse_IgnoresPercentageVariationTokens(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }
}
