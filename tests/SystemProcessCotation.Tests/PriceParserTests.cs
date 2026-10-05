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
    [InlineData("R $ 31,42", 31.42)]
    [InlineData("R \u00A0$ 31,42", 31.42)]
    [InlineData("BRL 31,42", 31.42)]
    public void TryParse_AcceptsSpacedCurrencyMarkers(string value, double expected)
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
    [InlineData("R$ 1 234,56", 1234.56)]
    [InlineData("Cotação R$ 12 345,67", 12345.67)]
    [InlineData("BRL 1\u00A0234,56", 1234.56)]
    public void TryParse_AcceptsWhitespaceThousandsGroups(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("1'234.56", 1234.56)]
    [InlineData("1’234,56", 1234.56)]
    [InlineData("R$ 12'345,67", 12345.67)]
    public void TryParse_AcceptsApostropheThousandsGroups(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Fact]
    public void TryParse_RejectsMalformedApostropheThousandsGroups()
    {
        var parsed = global::PriceParser.TryParse("1'23", out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("31,42 +0,50%", 31.42)]
    [InlineData("12/09/2026 31,42", 31.42)]
    [InlineData("Cotação em 12/09/2026: R$ 31,42", 31.42)]
    public void TryParse_ExtractsPriceFromMixedQuoteText(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Theory]
    [InlineData("+0,50%")]
    [InlineData("0,50 %")]
    public void TryParse_IgnoresStandalonePercentageChanges(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_SkipsPercentageChangeBeforePrice()
    {
        var parsed = global::PriceParser.TryParse("Alta +0,50% cotação R$ 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }

    [Fact]
    public void TryParse_SkipsUnicodeMinusVariationBeforePrice()
    {
        var parsed = global::PriceParser.TryParse("Baixa \u22120,50 Cotação R$ 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }

    [Theory]
    [InlineData("\u22120,50%")]
    [InlineData("\uFF0D0,50 %")]
    public void TryParse_IgnoresStandaloneUnicodeMinusPercentageChanges(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("12/09/2026")]
    [InlineData("12 / 09 / 2026")]
    public void TryParse_IgnoresStandaloneSlashDates(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("12-09-2026")]
    [InlineData("12 - 09 - 2026")]
    public void TryParse_IgnoresStandaloneHyphenDates(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_ExtractsPriceAfterHyphenDate()
    {
        var parsed = global::PriceParser.TryParse("12-09-2026 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }

    [Theory]
    [InlineData("12.09.2026")]
    [InlineData("12 . 09 . 2026")]
    public void TryParse_IgnoresStandaloneDottedDates(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_ExtractsPriceAfterDottedDate()
    {
        var parsed = global::PriceParser.TryParse("12.09.2026 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }

    [Theory]
    [InlineData("Atualizado 10:45")]
    [InlineData("Atualizado 10 : 45")]
    public void TryParse_IgnoresStandaloneTimeFragments(string value)
    {
        var parsed = global::PriceParser.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_StillReadsPriceAfterTimeText()
    {
        var parsed = global::PriceParser.TryParse("Atualizado 10:45 Cotação R$ 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }

    [Fact]
    public void TryParse_IgnoresTickerDigitsWhenNoPriceIsPresent()
    {
        var parsed = global::PriceParser.TryParse("PETR4", out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParse_StillReadsPriceAfterTickerText()
    {
        var parsed = global::PriceParser.TryParse("PETR4 cotação 31,42", out var price);

        Assert.True(parsed);
        Assert.Equal(31.42, price);
    }
}
