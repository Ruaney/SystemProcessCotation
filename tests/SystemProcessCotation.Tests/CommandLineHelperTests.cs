namespace SystemProcessCotation.Tests;

public class CommandLineHelperTests
{
    [Fact]
    public void ParseArguments_NormalizesSymbolAndPrices()
    {
        var settings = global::CommandLineHelper.ParseArguments([" petr4 ", "35.50", "30.25"]);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
        Assert.Equal(3000, settings.CheckIntervalMs);
        Assert.Equal(60, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ParseArguments_RejectsEmptySymbol()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments([" ", "35.50", "30.25"]));

        Assert.Contains("ativo", exception.Message);
    }

    [Fact]
    public void ParseArguments_RejectsInvalidThresholdOrder()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "30.00", "30.00"]));

        Assert.Contains("venda deve ser maior", exception.Message);
    }

    [Fact]
    public void ParseArguments_RejectsNonPositivePrices()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "35.00", "0"]));

        Assert.Contains("maiores que zero", exception.Message);
    }

    [Fact]
    public void NormalizeAndValidate_NormalizesSymbolAndDefaultsTiming()
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = " petr4 ",
            PriceToSell = 35.50,
            PriceToBuy = 30.25,
            CheckIntervalMs = 0,
            AlertCooldownSeconds = -1
        };

        settings.NormalizeAndValidate();

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(3000, settings.CheckIntervalMs);
        Assert.Equal(60, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void NormalizeAndValidate_RejectsInvalidConfiguredThresholds()
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = "PETR4",
            PriceToSell = 30.00,
            PriceToBuy = 30.00
        };

        var exception = Assert.Throws<ArgumentException>(() => settings.NormalizeAndValidate());

        Assert.Contains("venda deve ser maior", exception.Message);
    }
}
