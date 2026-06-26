
public interface ITradingService{
    Task<TradingAlert?> AnalyzeCotationAsync(CotationResult cotation, TradingSettings settings, CancellationToken cancellationToken = default);
}
