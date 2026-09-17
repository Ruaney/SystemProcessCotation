namespace SystemProcessCotation.Tests;

public class StockSymbolTests
{
    [Theory]
    [InlineData("petr4", "PETR4")]
    [InlineData("petr4.sa", "PETR4")]
    [InlineData("B3:PETR4", "PETR4")]
    [InlineData("bvmf:petr4", "PETR4")]
    [InlineData("BMFBOVESPA:PETR4.SA", "PETR4")]
    public void TryNormalize_AcceptsCommonBrazilianMarketFormats(string value, string expected)
    {
        var parsed = global::StockSymbol.TryNormalize(value, out var normalized);

        Assert.True(parsed);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("NYSE:PETR4")]
    [InlineData("PETR4.BA")]
    [InlineData("PETR-4")]
    public void TryNormalize_RejectsUnsupportedDecoratedSymbols(string value)
    {
        var parsed = global::StockSymbol.TryNormalize(value, out _);

        Assert.False(parsed);
    }
}
