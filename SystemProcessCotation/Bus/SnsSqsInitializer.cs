using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provisiona os tópicos SNS e as filas SQS (com a inscrição entre eles) antes
/// dos workers iniciarem, evitando perder mensagens publicadas antes da inscrição
/// e aguardando o LocalStack ficar pronto. Registrado como o primeiro hosted service.
/// </summary>
public class SnsSqsInitializer : IHostedService
{
    private readonly SnsSqsEventBus _bus;
    private readonly ILogger<SnsSqsInitializer> _logger;

    public SnsSqsInitializer(SnsSqsEventBus bus, ILogger<SnsSqsInitializer> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Provisionando tópicos SNS e filas SQS...");
        await _bus.EnsureChannelAsync(Channels.Cotations, cancellationToken);
        await _bus.EnsureChannelAsync(Channels.Alerts, cancellationToken);
        _logger.LogInformation("Barramento SNS/SQS pronto (canais: '{Cotations}', '{Alerts}')", Channels.Cotations, Channels.Alerts);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
