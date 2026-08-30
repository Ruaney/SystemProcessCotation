using System.Globalization;

public enum AlertType
{
    Buy,
    Sell
}

public class TradingAlert
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public AlertType Type { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public double CurrentPrice { get; set; }
    public double TargetPrice { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string GetMessage()
    {
        var action = Type == AlertType.Buy ? "Compra" : "Venda";
        return $"Alerta de {action} - {Symbol}\n\n" +
            $"Preço atual: R$ {FormatPrice(CurrentPrice)}\n" +
            $"Preço de referência configurado: R$ {FormatPrice(TargetPrice)}\n" +
            $"Recomendação: {action} {Symbol}\n" +
            $"Horário (UTC): {Timestamp.ToUniversalTime():dd/MM/yyyy HH:mm:ss}";
    }

    public string GetSubject()
    {
        var action = Type == AlertType.Buy ? "COMPRA" : "VENDA";
        return $"Alerta {action} - {Symbol} - R$ {FormatPrice(CurrentPrice)}";
    }

    private static string FormatPrice(double price) =>
        price.ToString("N2", BrazilianCulture);
}
