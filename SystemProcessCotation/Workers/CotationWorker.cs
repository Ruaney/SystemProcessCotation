using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Produtor: a cada intervalo busca a cotação do ativo e publica o resultado
/// no canal de cotações do barramento.
/// </summary>
public class CotationWorker : BackgroundService
{
    private readonly ICotationService _cotationService;
    private readonly IEventBus _bus;
    private readonly ILogger<CotationWorker> _logger;
    private readonly TradingSettings _settings;

    public CotationWorker(ICotationService cotationService, IEventBus bus, ILogger<CotationWorker> logger, TradingSettings settings)
    {
        _cotationService = cotationService;
        _bus = bus;
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = _settings.CheckIntervalMs > 0 ? _settings.CheckIntervalMs : 3000;
        _logger.LogInformation(
            "Monitorando {Symbol} | venda >= R$ {Sell:F2} | compra <= R$ {Buy:F2} | intervalo {Interval}ms",
            _settings.StockSymbol, _settings.PriceToSell, _settings.PriceToBuy, intervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
        try
        {
            do
            {
                try
                {
                    var cotation = await _cotationService.GetCotationAsync(_settings.StockSymbol, stoppingToken);
                    _logger.LogInformation("Cotação {Symbol}: R$ {Price:F2}", cotation.Symbol, cotation.Price);
                    await _bus.PublishAsync(Channels.Cotations, cotation, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter/publicar cotação");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Encerramento solicitado pelo host.
        }
    }
}
