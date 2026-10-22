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

    public bool IsValid =>
        Enum.IsDefined(Type)
        && StockSymbol.TryNormalize(Symbol, out _)
        && double.IsFinite(CurrentPrice)
        && CurrentPrice > 0
        && double.IsFinite(TargetPrice)
        && TargetPrice > 0
        && Timestamp != DateTime.MinValue;

    public string GetMessage()
    {
        var action = Type == AlertType.Buy ? "Compra" : "Venda";
        var symbol = FormatSymbol(Symbol);
        return $"Alerta de {action} - {symbol}\n\n" +
            $"Preço atual: R$ {FormatPrice(CurrentPrice)}\n" +
            $"Preço de referência configurado: R$ {FormatPrice(TargetPrice)}\n" +
            $"Recomendação: {action} {symbol}\n" +
            $"Horário (UTC): {FormatUtcTimestamp(Timestamp):dd/MM/yyyy HH:mm:ss}";
    }

    public string GetSubject()
    {
        var action = Type == AlertType.Buy ? "COMPRA" : "VENDA";
        return $"Alerta {action} - {FormatSymbol(Symbol)} - R$ {FormatPrice(CurrentPrice)}";
    }

    private static string FormatSymbol(string? symbol) =>
        StockSymbol.TryNormalize(symbol, out var normalized)
            ? normalized
            : (symbol ?? string.Empty).Trim().ToUpperInvariant();

    private static string FormatPrice(double price) =>
        price.ToString("N2", BrazilianCulture);

    private static DateTime FormatUtcTimestamp(DateTime timestamp) =>
        timestamp.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            : timestamp.ToUniversalTime();
}
