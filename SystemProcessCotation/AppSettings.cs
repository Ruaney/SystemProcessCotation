public class AppSettings
{
    public SmtpSettings SmtpSettings { get; set; } = new();
    public TradingSettings TradingSettings { get; set; } = new();
}

public class TradingSettings
{
    private const int DefaultCheckIntervalMs = 3000;
    private const int DefaultAlertCooldownSeconds = 60;

    public string StockSymbol { get; set; } = string.Empty;
    public double PriceToSell { get; set; }
    public double PriceToBuy { get; set; }
    public int CheckIntervalMs { get; set; }
    public int AlertCooldownSeconds { get; set; } = DefaultAlertCooldownSeconds;

    public TradingSettings NormalizeAndValidate()
    {
        var normalizedSymbol = StockSymbol?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedSymbol))
        {
            throw new ArgumentException("O código do ativo é obrigatório.");
        }

        if (PriceToSell <= 0 || PriceToBuy <= 0)
        {
            throw new ArgumentException("Os preços devem ser maiores que zero.");
        }

        if (PriceToSell <= PriceToBuy)
        {
            throw new ArgumentException("O preço de venda deve ser maior que o preço de compra.");
        }

        StockSymbol = normalizedSymbol;
        CheckIntervalMs = CheckIntervalMs > 0 ? CheckIntervalMs : DefaultCheckIntervalMs;
        AlertCooldownSeconds = AlertCooldownSeconds > 0 ? AlertCooldownSeconds : DefaultAlertCooldownSeconds;

        return this;
    }
}
