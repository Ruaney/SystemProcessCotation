using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Consumidor: ouve os alertas no barramento e envia o email.
/// Se o SMTP não estiver configurado, apenas registra o alerta no log,
/// mantendo o sistema demonstrável sem credenciais.
/// </summary>
public class NotificationWorker : BackgroundService
{
    private readonly IEventBus _bus;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly SmtpSettings _smtp;

    public NotificationWorker(IEventBus bus, IEmailService emailService, ILogger<NotificationWorker> logger, SmtpSettings smtp)
    {
        _bus = bus;
        _emailService = emailService;
        _logger = logger;
        _smtp = smtp;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bus.SubscribeAsync<TradingAlert>(Channels.Alerts, HandleAlertAsync, stoppingToken);
        _logger.LogInformation("Inscrito no canal '{Channel}', aguardando alertas...", Channels.Alerts);
    }

    private async Task HandleAlertAsync(TradingAlert alert, CancellationToken cancellationToken)
    {
        if (!_smtp.IsConfigured)
        {
            _logger.LogWarning(
                "SMTP não configurado — alerta apenas registrado.\nAssunto: {Subject}\n{Message}",
                alert.GetSubject(), alert.GetMessage());
            return;
        }

        try
        {
            await _emailService.SendAlertAsync(
                _smtp.ToAddress, _smtp.FromAddress, alert.GetSubject(), alert.GetMessage(), _smtp, cancellationToken);
            _logger.LogInformation("Email enviado para {To} → {Subject}", _smtp.ToAddress, alert.GetSubject());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email de alerta");
        }
    }
}
