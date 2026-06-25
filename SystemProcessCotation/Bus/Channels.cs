/// <summary>
/// Nomes dos canais do barramento de mensagens (Redis Pub/Sub).
/// </summary>
public static class Channels
{
    /// <summary>Cotações coletadas e publicadas pelo produtor.</summary>
    public const string Cotations = "cotations";

    /// <summary>Alertas de compra/venda prontos para notificação.</summary>
    public const string Alerts = "alerts";
}
