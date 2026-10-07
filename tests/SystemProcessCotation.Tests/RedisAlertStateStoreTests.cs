namespace SystemProcessCotation.Tests;

public class RedisAlertStateStoreTests
{
    [Fact]
    public void BuildKeys_NormalizesSymbolBeforeCreatingRedisKeys()
    {
        var alert = new global::TradingAlert
        {
            Type = global::AlertType.Buy,
            Symbol = " petr4.sa ",
            CurrentPrice = 29.90
        };

        var keys = global::RedisAlertStateStore.BuildKeys(alert);

        Assert.Equal("lastalertprice:PETR4:Buy", keys.LastPriceKey);
        Assert.Equal("alertcd:PETR4:Buy", keys.CooldownKey);
    }

    [Fact]
    public void BuildKeys_RejectsInvalidSymbolsBeforeCreatingRedisKeys()
    {
        var alert = new global::TradingAlert
        {
            Type = global::AlertType.Sell,
            Symbol = "PETR4.BR",
            CurrentPrice = 35.00
        };

        Assert.Throws<ArgumentException>(() => global::RedisAlertStateStore.BuildKeys(alert));
    }
}
