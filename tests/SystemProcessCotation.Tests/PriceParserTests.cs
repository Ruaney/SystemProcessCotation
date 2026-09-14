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
    [InlineData("31,42 +0,50%", 31.42)]
    [InlineData("R$ 1.234,56 (fechamento)", 1234.56)]
    public void TryParse_UsesFirstPriceTokenFromMixedText(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("-31,42", -31.42)]
    [InlineData("R$ -1.234,56 suspenso", -1234.56)]
    public void TryParse_PreservesLeadingSign(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }
}
