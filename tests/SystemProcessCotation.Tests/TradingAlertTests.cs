namespace SystemProcessCotation.Tests;

public class TradingAlertTests
{
    [Fact]
    public void Constructor_DefaultsTimestampToUtc()
    {
        var alert = new global::TradingAlert();

        Assert.Equal(DateTimeKind.Utc, alert.Timestamp.Kind);
    }

    [Fact]
    public void GetMessage_IncludesUtcTimestampAndTrimmedRecommendation()
    {
        var alert = new global::TradingAlert
        {
            Type = global::AlertType.Buy,
            Symbol = "PETR4",
            CurrentPrice = 29.90,
            TargetPrice = 30.00,
            Timestamp = new DateTime(2026, 8, 28, 12, 30, 0, DateTimeKind.Utc)
        };

        var message = alert.GetMessage();

        Assert.Contains("Preço atual: R$ 29,90", message);
        Assert.Contains("Preço de referência configurado: R$ 30,00", message);
        Assert.Contains("Recomendação: Compra PETR4\n", message);
        Assert.Contains("Horário (UTC): 28/08/2026 12:30:00", message);
    }

    [Fact]
    public void GetSubject_FormatsPriceWithBrazilianDecimalSeparator()
    {
        var alert = new global::TradingAlert
        {
            Type = global::AlertType.Sell,
            Symbol = "PETR4",
            CurrentPrice = 1234.56
        };

        Assert.Equal("Alerta VENDA - PETR4 - R$ 1.234,56", alert.GetSubject());
    }

    [Fact]
    public void GetMessageAndSubject_NormalizeSymbolForDisplay()
    {
        var alert = new global::TradingAlert
        {
            Type = global::AlertType.Buy,
            Symbol = " petr4 ",
            CurrentPrice = 29.90,
            TargetPrice = 30.00,
            Timestamp = new DateTime(2026, 8, 28, 12, 30, 0, DateTimeKind.Utc)
        };

        Assert.Contains("Alerta de Compra - PETR4", alert.GetMessage());
        Assert.Contains("Recomendação: Compra PETR4\n", alert.GetMessage());
        Assert.Equal("Alerta COMPRA - PETR4 - R$ 29,90", alert.GetSubject());
    }
}
