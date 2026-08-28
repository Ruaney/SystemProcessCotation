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

        Assert.Contains("Recomendação: Compra PETR4\n", message);
        Assert.Contains("Horário (UTC): 28/08/2026 12:30:00", message);
    }
}
