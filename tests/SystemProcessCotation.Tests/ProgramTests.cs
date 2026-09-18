using Microsoft.Extensions.Configuration;

namespace SystemProcessCotation.Tests;

public class ProgramTests
{
    [Fact]
    public void ResolveTradingSettings_AcceptsLocalizedConfiguredPrices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Trading:StockSymbol"] = "petr4",
                ["Trading:PriceToSell"] = "R$ 35,50",
                ["Trading:PriceToBuy"] = "R$ 30,25",
                ["Trading:CheckIntervalMs"] = "1500",
                ["Trading:AlertCooldownSeconds"] = "20"
            })
            .Build();

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
        Assert.Equal(1500, settings.CheckIntervalMs);
        Assert.Equal(20, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_ReportsInvalidConfiguredPrices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Trading:StockSymbol"] = "PETR4",
                ["Trading:PriceToSell"] = "indisponivel",
                ["Trading:PriceToBuy"] = "30,25"
            })
            .Build();

        var exception = Assert.Throws<ArgumentException>(
            () => global::Program.ResolveTradingSettings([], configuration));

        Assert.Contains("preço de venda", exception.Message);
        Assert.Contains("22,67", exception.Message);
    }
}
