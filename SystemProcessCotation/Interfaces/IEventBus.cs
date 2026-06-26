/// <summary>
/// Barramento de mensagens assíncrono. Desacopla os workers: o produtor publica
/// eventos em um canal e os consumidores se inscrevem sem se conhecerem.
/// </summary>
public interface IEventBus
{
    /// <summary>Publica um evento serializado em JSON no canal informado.</summary>
    Task PublishAsync<T>(string channel, T message, CancellationToken cancellationToken = default);

    /// <summary>Inscreve um handler assíncrono para processar os eventos do canal.</summary>
    Task SubscribeAsync<T>(string channel, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}
