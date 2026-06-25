using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

/// <summary>
/// Implementação do <see cref="IEventBus"/> sobre Redis Pub/Sub.
/// Os eventos trafegam como JSON; cada inscrição processa as mensagens em ordem.
/// </summary>
public class RedisEventBus : IEventBus
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ISubscriber _subscriber;
    private readonly ILogger<RedisEventBus> _logger;

    public RedisEventBus(IConnectionMultiplexer connection, ILogger<RedisEventBus> logger)
    {
        _subscriber = connection.GetSubscriber();
        _logger = logger;
    }

    public async Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(message, JsonOptions);
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), payload);
        _logger.LogDebug("→ publicado em '{Channel}': {Payload}", channel, payload);
    }

    public async Task SubscribeAsync<T>(string channel, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {
        var queue = await _subscriber.SubscribeAsync(RedisChannel.Literal(channel));
        queue.OnMessage(async message =>
        {
            if (message.Message.IsNullOrEmpty)
            {
                return;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<T>(message.Message!, JsonOptions);
                if (payload is not null)
                {
                    await handler(payload, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem do canal '{Channel}'", channel);
            }
        });
    }
}
