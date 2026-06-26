using Microsoft.Extensions.Logging;

/// <summary>
/// Regra de negócio pura: decide se a cotação dispara um alerta de compra ou venda.
/// A deduplicação (preço mudou + cooldown) ficou a cargo do <see cref="IAlertStateStore"/>,
/// consumido pelo <see cref="TradingWorker"/>.
/// </summary>
public class TradingService : ITradingService
{
    private readonly ILogger<TradingService> _logger;

    public TradingService(ILogger<TradingService> logger)
    {
        _logger = logger;
    }

    public Task<TradingAlert?> AnalyzeCotationAsync(CotationResult cotation, TradingSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!cotation.IsValid)
        {
            return Task.FromResult<TradingAlert?>(null);
        }

        if (cotation.Price < settings.PriceToBuy && cotation.Price < settings.PriceToSell)
        {
            _logger.LogWarning("Informe preços de compra/venda em torno do preço atual do ativo para alertas mais consistentes.");
        }

        TradingAlert? alert = null;
        if (cotation.Price >= settings.PriceToSell)
        {
            alert = new TradingAlert
            {
                Type = AlertType.Sell,
                Symbol = cotation.Symbol,
                CurrentPrice = cotation.Price,
                TargetPrice = settings.PriceToSell
            };
            _logger.LogInformation("VENDA: {Symbol} R$ {Price:F2} (alvo: R$ {Target:F2})", cotation.Symbol, cotation.Price, settings.PriceToSell);
        }
        else if (cotation.Price <= settings.PriceToBuy)
        {
            alert = new TradingAlert
            {
                Type = AlertType.Buy,
                Symbol = cotation.Symbol,
                CurrentPrice = cotation.Price,
                TargetPrice = settings.PriceToBuy
            };
            _logger.LogInformation("COMPRA: {Symbol} R$ {Price:F2} (alvo: R$ {Target:F2})", cotation.Symbol, cotation.Price, settings.PriceToBuy);
        }

        return Task.FromResult(alert);
    }
}
