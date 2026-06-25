/// <summary>
/// Estado compartilhado dos alertas, mantido no Redis (em vez de memória local),
/// para evitar spam: só alerta quando o preço mudou desde o último alerta do mesmo
/// tipo e o cooldown já expirou.
/// </summary>
public interface IAlertStateStore
{
    /// <summary>
    /// Retorna <c>true</c> e registra o alerta quando ele deve ser enviado
    /// (preço diferente do último alerta e fora do período de cooldown).
    /// </summary>
    Task<bool> ShouldAlertAsync(TradingAlert alert, TimeSpan cooldown);
}
