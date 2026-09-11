public class CotationResult
{
    public string Symbol { get; set; } = string.Empty;
    public double Price { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsValid =>
        double.IsFinite(Price)
        && Price > 0
        && StockSymbol.TryNormalize(Symbol, out _);
}
