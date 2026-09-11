using Microsoft.Extensions.Logging.Abstractions;

namespace SystemProcessCotation.Tests;

public class TradingServiceTests
{
    private readonly global::TradingService _service = new(NullLogger<global::TradingService>.Instance);

    [Fact]
    public async Task AnalyzeCotationAsync_ReturnsSellAlertWhenPriceReachesSellThreshold()
    {
        var alert = await _service.AnalyzeCotationAsync(
            new global::CotationResult { Symbol = "PETR4", Price = 35.00 },
            new global::TradingSettings { PriceToSell = 35.00, PriceToBuy = 30.00 });

        Assert.NotNull(alert);
        Assert.Equal(global::AlertType.Sell, alert.Type);
        Assert.Equal("PETR4", alert.Symbol);
        Assert.Equal(35.00, alert.CurrentPrice);
        Assert.Equal(35.00, alert.TargetPrice);
    }

    [Fact]
    public async Task AnalyzeCotationAsync_NormalizesSymbolOnAlert()
    {
        var alert = await _service.AnalyzeCotationAsync(
            new global::CotationResult { Symbol = " petr4 ", Price = 35.00 },
            new global::TradingSettings { PriceToSell = 35.00, PriceToBuy = 30.00 });

        Assert.NotNull(alert);
        Assert.Equal("PETR4", alert.Symbol);
    }

    [Fact]
    public async Task AnalyzeCotationAsync_ReturnsBuyAlertWhenPriceReachesBuyThreshold()
    {
        var alert = await _service.AnalyzeCotationAsync(
            new global::CotationResult { Symbol = "PETR4", Price = 29.90 },
            new global::TradingSettings { PriceToSell = 35.00, PriceToBuy = 30.00 });

        Assert.NotNull(alert);
        Assert.Equal(global::AlertType.Buy, alert.Type);
        Assert.Equal(29.90, alert.CurrentPrice);
        Assert.Equal(30.00, alert.TargetPrice);
    }

    [Fact]
    public async Task AnalyzeCotationAsync_ReturnsNullWhenPriceIsInsideRange()
    {
        var alert = await _service.AnalyzeCotationAsync(
            new global::CotationResult { Symbol = "PETR4", Price = 32.00 },
            new global::TradingSettings { PriceToSell = 35.00, PriceToBuy = 30.00 });

        Assert.Null(alert);
    }

    [Fact]
    public async Task AnalyzeCotationAsync_ReturnsNullForInvalidCotation()
    {
        var alert = await _service.AnalyzeCotationAsync(
            new global::CotationResult { Symbol = "", Price = 0 },
            new global::TradingSettings { PriceToSell = 35.00, PriceToBuy = 30.00 });

        Assert.Null(alert);
    }
}
