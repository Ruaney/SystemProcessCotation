using System.Collections.Concurrent;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementação do <see cref="IEventBus"/> sobre AWS SNS + SQS.
/// Cada canal vira um tópico SNS (fan-out) com uma fila SQS inscrita (entrega durável):
/// publicar = <c>sns:Publish</c>; consumir = long-poll na fila SQS + delete da mensagem.
/// </summary>
public class SnsSqsEventBus : IEventBus
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAmazonSimpleNotificationService _sns;
    private readonly IAmazonSQS _sqs;
    private readonly ILogger<SnsSqsEventBus> _logger;

    private readonly ConcurrentDictionary<string, string> _topicArns = new();
    private readonly ConcurrentDictionary<string, string> _queueUrls = new();

    public SnsSqsEventBus(IAmazonSimpleNotificationService sns, IAmazonSQS sqs, ILogger<SnsSqsEventBus> logger)
    {
        _sns = sns;
        _sqs = sqs;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default)
    {
        var topicArn = await GetOrCreateTopicArnAsync(channel, cancellationToken);
        var payload = JsonSerializer.Serialize(message, JsonOptions);
        await _sns.PublishAsync(new PublishRequest { TopicArn = topicArn, Message = payload }, cancellationToken);
        _logger.LogDebug("→ publicado no tópico SNS '{Channel}': {Payload}", channel, payload);
    }

    public async Task SubscribeAsync<T>(string channel, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {
        var queueUrl = await EnsureChannelAsync(channel, cancellationToken);
        _logger.LogInformation("Consumindo a fila SQS '{Channel}'", channel);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                }, cancellationToken);

                if (response.Messages is null)
                {
                    continue;
                }

                foreach (var sqsMessage in response.Messages)
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<T>(sqsMessage.Body, JsonOptions);
                        if (payload is not null)
                        {
                            await handler(payload, cancellationToken);
                        }
                        await _sqs.DeleteMessageAsync(queueUrl, sqsMessage.ReceiptHandle, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem da fila '{Channel}'", channel);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Falha temporária consumindo '{Channel}': {Message}", channel, ex.Message);
                await SafeDelay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Garante (de forma idempotente, com retry para o cold start do LocalStack) que o
    /// tópico SNS, a fila SQS e a inscrição entre eles existem. Retorna a URL da fila.
    /// </summary>
    public Task<string> EnsureChannelAsync(string channel, CancellationToken cancellationToken) =>
        RetryAsync(() => SetupChannelAsync(channel, cancellationToken), channel, cancellationToken);

    private async Task<string> SetupChannelAsync(string channel, CancellationToken ct)
    {
        var topicArn = await GetOrCreateTopicArnAsync(channel, ct);

        var queueUrl = _queueUrls.TryGetValue(channel, out var cachedUrl)
            ? cachedUrl
            : (await _sqs.CreateQueueAsync(channel, ct)).QueueUrl;
        _queueUrls[channel] = queueUrl;

        var attributes = await _sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames = new List<string> { "QueueArn" }
        }, ct);
        var queueArn = attributes.Attributes["QueueArn"];

        await _sqs.SetQueueAttributesAsync(new SetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            Attributes = new Dictionary<string, string> { ["Policy"] = BuildQueuePolicy(queueArn, topicArn) }
        }, ct);

        await _sns.SubscribeAsync(new SubscribeRequest
        {
            TopicArn = topicArn,
            Protocol = "sqs",
            Endpoint = queueArn,
            ReturnSubscriptionArn = true,
            // entrega o corpo publicado "cru", sem o envelope JSON do SNS.
            Attributes = new Dictionary<string, string> { ["RawMessageDelivery"] = "true" }
        }, ct);

        return queueUrl;
    }

    private async Task<string> GetOrCreateTopicArnAsync(string channel, CancellationToken ct)
    {
        if (_topicArns.TryGetValue(channel, out var cached))
        {
            return cached;
        }

        var response = await _sns.CreateTopicAsync(new CreateTopicRequest { Name = channel }, ct);
        _topicArns[channel] = response.TopicArn;
        return response.TopicArn;
    }

    private static string BuildQueuePolicy(string queueArn, string topicArn) => $$"""
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Effect": "Allow",
          "Principal": { "Service": "sns.amazonaws.com" },
          "Action": "sqs:SendMessage",
          "Resource": "{{queueArn}}",
          "Condition": { "ArnEquals": { "aws:SourceArn": "{{topicArn}}" } }
        }
      ]
    }
    """;

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, string channel, CancellationToken ct, int attempts = 15, int delayMs = 2000)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < attempts && !ct.IsCancellationRequested)
            {
                _logger.LogWarning("Aguardando SNS/SQS para '{Channel}' (tentativa {Attempt}): {Message}", channel, attempt, ex.Message);
                await SafeDelay(TimeSpan.FromMilliseconds(delayMs), ct);
            }
        }
    }

    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
