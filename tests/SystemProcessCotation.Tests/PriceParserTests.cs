namespace SystemProcessCotation.Tests;

public class PriceParserTests
{
    [Theory]
    [InlineData("1.234", 1234.00)]
    [InlineData("12.345", 12345.00)]
    [InlineData("1.234.567", 1234567.00)]
    [InlineData("R$ 1.234", 1234.00)]
    public void TryParse_PrefersBrazilianThousandsWhenDotGroupsHaveThreeDigits(string value, double expected)
    {
        var parsed = global::PriceParser.TryParse(value, out var price);

        Assert.True(parsed);
        Assert.Equal(expected, price);
    }

    [Fact]
    public void TryParse_KeepsInvariantDecimalWhenDotFractionHasTwoDigits()
    {
        var parsed = global::PriceParser.TryParse("35.50", out var price);

        Assert.True(parsed);
        Assert.Equal(35.50, price);
    }
}
