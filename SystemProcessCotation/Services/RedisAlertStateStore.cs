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
        var priceText = alert.CurrentPrice.ToString("R", CultureInfo.InvariantCulture);
        var lastPriceKey = $"lastalertprice:{alert.Symbol}:{alert.Type}";
        var cooldownKey = $"alertcd:{alert.Symbol}:{alert.Type}";

        // 1) mesmo preço do último alerta deste tipo? não repete.
        var lastPrice = await _db.StringGetAsync(lastPriceKey);
        if (!lastPrice.IsNullOrEmpty && lastPrice == priceText)
        {
            return false;
        }

        // 2) ainda dentro do cooldown? SET NX só vence quando a chave não existe.
        var slotAcquired = await _db.StringSetAsync(cooldownKey, priceText, cooldown, When.NotExists);
        if (!slotAcquired)
        {
            return false;
        }

        // 3) registra o preço deste alerta e libera o envio.
        await _db.StringSetAsync(lastPriceKey, priceText);
        return true;
    }
}
