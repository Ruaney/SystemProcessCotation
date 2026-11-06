namespace SystemProcessCotation.Tests;

public class StockSymbolTests
{
    [Theory]
    [InlineData("bvmf:petr4", "PETR4")]
    [InlineData("B3:PETR4.SA", "PETR4")]
    [InlineData("B3 : PETR4", "PETR4")]
    [InlineData("BOVESPA:PETR4", "PETR4")]
    [InlineData("BMFBOVESPA:PETR4.SA", "PETR4")]
    public void TryNormalize_RemovesB3ProviderAffixes(string symbol, string expected)
    {
        var parsed = global::StockSymbol.TryNormalize(symbol, out var normalized);

        Assert.True(parsed);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("petr4.b3", "PETR4")]
    [InlineData("PETR4.BVMF", "PETR4")]
    [InlineData("PETR4.BOVESPA", "PETR4")]
    [InlineData("PETR4.BMFBOVESPA", "PETR4")]
    public void TryNormalize_RemovesB3ProviderSuffixes(string symbol, string expected)
    {
        var parsed = global::StockSymbol.TryNormalize(symbol, out var normalized);

        Assert.True(parsed);
        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void TryNormalize_RemovesRepeatedProviderAffixes()
    {
        var parsed = global::StockSymbol.TryNormalize("B3:BVMF:PETR4.SA.B3", out var normalized);

        Assert.True(parsed);
        Assert.Equal("PETR4", normalized);
    }

    [Fact]
    public void TryNormalize_RejectsEmptySymbolAfterB3Prefix()
    {
        var parsed = global::StockSymbol.TryNormalize("BVMF:", out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("\u200BPETR4")]
    [InlineData("PETR4\uFEFF")]
    [InlineData("PETR\u20604")]
    public void TryNormalize_RemovesCopiedFormatWhitespace(string symbol)
    {
        var parsed = global::StockSymbol.TryNormalize(symbol, out var normalized);

        Assert.True(parsed);
        Assert.Equal("PETR4", normalized);
    }

    [Theory]
    [InlineData("PETR-4")]
    [InlineData("VALE 3")]
    public void NormalizeOrThrow_PreservesTheProvidedParameterNameForInvalidSymbols(string symbol)
    {
        var error = Assert.Throws<ArgumentException>(() => global::StockSymbol.NormalizeOrThrow(symbol, "asset"));

        Assert.Equal("asset", error.ParamName);
    }
}
