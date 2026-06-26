using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Consumidor/produtor: ouve as cotações no barramento, decide se há compra/venda,
/// aplica a deduplicação (preço mudou + cooldown) via Redis e publica os alertas.
/// </summary>
public class TradingWorker : BackgroundService
{
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromMinutes(1);

    private readonly IEventBus _bus;
    private readonly ITradingService _tradingService;
    private readonly IAlertStateStore _state;
    private readonly ILogger<TradingWorker> _logger;
    private readonly TradingSettings _settings;

    public TradingWorker(IEventBus bus, ITradingService tradingService, IAlertStateStore state, ILogger<TradingWorker> logger, TradingSettings settings)
    {
        _bus = bus;
        _tradingService = tradingService;
        _state = state;
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bus.SubscribeAsync<CotationResult>(Channels.Cotations, HandleCotationAsync, stoppingToken);
        _logger.LogInformation("Inscrito no canal '{Channel}', aguardando cotações...", Channels.Cotations);
    }

    private async Task HandleCotationAsync(CotationResult cotation, CancellationToken cancellationToken)
    {
        var alert = await _tradingService.AnalyzeCotationAsync(cotation, _settings, cancellationToken);
        if (alert is null)
        {
            return;
        }

        if (!await _state.ShouldAlertAsync(alert, AlertCooldown))
        {
            _logger.LogDebug("Alerta de {Type} para {Symbol} ignorado (preço repetido ou em cooldown)", alert.Type, alert.Symbol);
            return;
        }

        _logger.LogInformation(
            "Alerta de {Type} para {Symbol} a R$ {Price:F2} → publicando em '{Channel}'",
            alert.Type, alert.Symbol, alert.CurrentPrice, Channels.Alerts);
        await _bus.PublishAsync(Channels.Alerts, alert, cancellationToken);
    }
}
