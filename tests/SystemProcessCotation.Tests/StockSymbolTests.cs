namespace SystemProcessCotation.Tests;

public class StockSymbolTests
{
    [Theory]
    [InlineData("bvmf:petr4", "PETR4")]
    [InlineData("B3:PETR4.SA", "PETR4")]
    public void TryNormalize_RemovesB3ProviderAffixes(string symbol, string expected)
    {
        var parsed = global::StockSymbol.TryNormalize(symbol, out var normalized);

        Assert.True(parsed);
        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void TryNormalize_RejectsEmptySymbolAfterB3Prefix()
    {
        var parsed = global::StockSymbol.TryNormalize("BVMF:", out _);

        Assert.False(parsed);
    }
}
