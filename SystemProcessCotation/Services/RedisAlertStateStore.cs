using System.Globalization;
using StackExchange.Redis;

/// <summary>
/// Implementação do <see cref="IAlertStateStore"/> usando chaves do Redis.
/// Substitui o dicionário em memória que antes vivia no <see cref="TradingService"/>,
/// tornando o estado assíncrono e compartilhável entre instâncias.
/// </summary>
public class RedisAlertStateStore : IAlertStateStore
{
    private readonly IDatabase _db;

    public RedisAlertStateStore(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
    }

    public async Task<bool> ShouldAlertAsync(TradingAlert alert, TimeSpan cooldown)
    {
        var keys = BuildKeys(alert);
        var priceText = alert.CurrentPrice.ToString("R", CultureInfo.InvariantCulture);

        // 1) mesmo preço do último alerta deste tipo? não repete.
        var lastPrice = await _db.StringGetAsync(keys.LastPriceKey);
        if (!lastPrice.IsNullOrEmpty && lastPrice == priceText)
        {
            return false;
        }

        // 2) ainda dentro do cooldown? SET NX só vence quando a chave não existe.
        var slotAcquired = await _db.StringSetAsync(keys.CooldownKey, priceText, cooldown, When.NotExists);
        if (!slotAcquired)
        {
            return false;
        }

        // 3) registra o preço deste alerta e libera o envio.
        await _db.StringSetAsync(keys.LastPriceKey, priceText);
        return true;
    }

    internal static AlertStateKeys BuildKeys(TradingAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var symbol = StockSymbol.NormalizeOrThrow(alert.Symbol, nameof(alert));
        return new AlertStateKeys(
            $"lastalertprice:{symbol}:{alert.Type}",
            $"alertcd:{symbol}:{alert.Type}");
    }

    internal readonly record struct AlertStateKeys(string LastPriceKey, string CooldownKey);
}
